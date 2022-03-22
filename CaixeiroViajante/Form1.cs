using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaixeiroViajante
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
        private void addMarcador(PointLatLng pontos)
        {
            GMapMarker marcador;
            marcador = new GMarkerGoogle(pontos, GMarkerGoogleType.red_dot);//cria o marcador
            GMapOverlay marcadores = new GMapOverlay("marcadores");
            marcadores.Markers.Add(marcador);//add na lista
            Mapa.Overlays.Add(marcadores);//sobrepoem os marcadores no mapa 
            Mapa.UpdateMarkerLocalPosition(marcador);//atualiza o marcador
        }
        private void PrepararMapa()
        {
            GMapProviders.GoogleMap.ApiKey = AppConfig.Key;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            Mapa.CacheLocation = @"cache";
            Mapa.ShowCenter = false;//tira o + do meio
            Mapa.DragButton = MouseButtons.Left;
            Mapa.MapProvider = GMapProviders.GoogleMap;
            Mapa.MinZoom = 5;
            Mapa.MaxZoom = 100;
            Mapa.Zoom = 13.3;
            Mapa.SetPositionByKeywords("Museu Oscar Niemeyer, Curitiba");//centraliza o mapa no museu
        }

        private double MarcarRota(PointLatLng pontoA, PointLatLng pontoB, Color cor, double dist)
        {

            var rota = GoogleMapProvider.Instance.GetRoute(pontoA, pontoB, false, false, 14);//calcula o trajeto
            var r = new GMapRoute(rota.Points, "Minha rota")
            {
                Stroke = new Pen(cor, 4)
            };
            var rotas = new GMapOverlay("rota");
            dist += rota.Distance;
            rotas.Routes.Add(r);
            Mapa.Overlays.Add(rotas);//sobrepoem a rota no mapa
            Mapa.UpdateRouteLocalPosition(r);//atualiza a rota
            return dist;
        }
        private void CalcularRota(double[,] matriz, int[] caminho, int paradas)
        {
            int[] inseridos = new int[paradas];//array com o mesmo tamanho do numero de elementos
            for (int i = 0; i < paradas; i++)
            {
                double refencia = int.MaxValue; //int max apenas para iniciar com um valor maior que 0 e maior que todos os outros possiveis
                int prox = 0;
                for (int j = 0; j < paradas; j++)
                {
                    if (!inseridos.Contains(j) && refencia > matriz[i, j]) //verifica se o local já não foi percorrido
                    {
                        prox = j;
                        refencia = matriz[i, j];//o valor atual passa a ser a referencia 
                    }
                }
                caminho[i + 1] = prox;
                inseridos[prox] = prox;//add na lista de locais
            }
        }
        private void ExibirRota(int n, int[] caminho, List<PointLatLng> paradas, Color cor)
        {
            int i; double dist = 0;
            for (i = 0; i < n; i++)
            {
                if (i < paradas.Count)
                {
                    dist += MarcarRota(paradas[caminho[i]], paradas[caminho[i + 1]], cor, 0);
                    addMarcador(paradas[caminho[i]]);
                }
            }
            lblKM.Text = "Distância: " + Math.Round(dist, 2) + " km";
        }
        public double GetDistancia(PointLatLng pontoA, PointLatLng pontoB)
        {
            double distancia = 0;
            MapRoute rota = GoogleMapProvider.Instance.GetRoute(pontoA, pontoB, false, false, 15);
            distancia += rota.Distance;
            return distancia;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            PrepararMapa();
            List<PointLatLng> pontos = new List<PointLatLng>();
            pontos.Add(new PointLatLng(-25.423606, -49.306911));//Barigui     0 = 7.5km
            pontos.Add(new PointLatLng(-25.388640, -49.305742));//Tingui      1 = 3.1km
            pontos.Add(new PointLatLng(-25.379102, -49.282396));//Tangua      2 = 2.1km
            pontos.Add(new PointLatLng(-25.384569, -49.276173));//Op. Arame   3 = 4.1km
            pontos.Add(new PointLatLng(-25.409516, -49.267407));//Oscar N.    4 = 6.7km
            pontos.Add(new PointLatLng(-25.441111, -49.237789));//Jardim Bot. 5 = 9.1km
            pontos.Add(new PointLatLng(-25.423606, -49.306911));//Barigui     
            double[,] matrizParadas = new double[pontos.Count, pontos.Count];
            Console.WriteLine("\nPossíveis Distâncias\n");
            for (int i = 0; i < pontos.Count; i++)
            {
                if (i < pontos.Count - 1)
                {
                    for (int j = 0; j < pontos.Count; j++)
                    {
                        double distancia = Math.Round(GetDistancia(pontos[i], pontos[j]), 1);//distancia de todas as combinações entre os pontos
                        matrizParadas[i, j] = distancia;//coloca na matriz as distancias
                        Console.Write("\t{0}", matrizParadas[i, j].ToString().Replace(',', '.') + ", ");
                    }
                    Console.WriteLine();
                }
            }
            int paradas = pontos.Count;//n de paradas/ vertices
            int[] caminhoIni = new int[paradas + 1];
            CalcularRota(matrizParadas, caminhoIni, paradas);
            ExibirRota(paradas + 1, caminhoIni, pontos, Color.FromArgb(10, 100, 200));
        }
    }
}





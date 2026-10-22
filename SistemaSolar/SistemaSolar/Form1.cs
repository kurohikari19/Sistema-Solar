using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaSolar
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        double anguloOrbita = 0;
        double anguloRotacionSol = 0;
        double anguloRotacionPlaneta = 0;

        public class vector
        {
            public double X, Y, Z;
            public vector(double x, double y, double z) { X = x; Y = y; Z = z; }
        }

        List<vector> esferaVertices = new List<vector>();
        List<int[]> esferaBordes = new List<int[]>();

        public Form1()
        {
            InitializeComponent();

            this.Width = 1000;
            this.Height = 700;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            GenerarEsfera(50, 15, 15);

            timer.Interval = 16; // ~60 FPS
            timer.Tick += Update;
            timer.Start();
        }

        void GenerarEsfera(double radio, int paralelos, int meridianos)
        {
            for (int i = 0; i <= paralelos; i++)
            {
                double latitud = Math.PI * i / paralelos;
                for (int j = 0; j <= meridianos; j++)
                {
                    double longitud = 2 * Math.PI * j / meridianos;

                    double x = radio * Math.Sin(latitud) * Math.Cos(longitud);
                    double y = radio * Math.Cos(latitud);
                    double z = radio * Math.Sin(latitud) * Math.Sin(longitud);

                    esferaVertices.Add(new vector(x, y, z));
                }
            }

            for (int i = 0; i < paralelos; i++)
            {
                for (int j = 0; j < meridianos; j++)
                {
                    int actual = i * (meridianos + 1) + j;
                    int siguiente = actual + 1;
                    int abajo = (i + 1) * (meridianos + 1) + j;

                    esferaBordes.Add(new int[] { actual, siguiente });
                    esferaBordes.Add(new int[] { actual, abajo });
                }
            }
        }

        private void Update(object sender, EventArgs e)
        {
            anguloOrbita += 0.01;           // Velocidad base de traslación
            anguloRotacionSol += 0.01;      // El Sol gira sobre su eje
            anguloRotacionPlaneta += 0.05;  // Los planetas giran más rápido sobre su eje

            Invalidate();
        }

        PointF Project(vector v)
        {
            double distancia = 400; // Distancia focal de la cámara
            double factor = distancia / (distancia + v.Z);

            double x = v.X * factor;
            double y = v.Y * factor;

            return new PointF(
                (float)(x + this.ClientSize.Width / 2),
                (float)(y + this.ClientSize.Height / 2)
            );
        }

        // Rotación en el eje Y
        vector rotacion(vector v, double anguloRotacion)
        {
            double cosY = Math.Cos(anguloRotacion);
            double sinY = Math.Sin(anguloRotacion);

            double x1 = v.X * cosY - v.Z * sinY;
            double y1 = v.Y;
            double z1 = v.X * sinY + v.Z * cosY;

            return new vector(x1, y1, z1);
        }

        // Método auxiliar para calcular la posición en órbita de cada planeta
        vector CalcularPosicionOrbita(double radioDeOrbita, double angulo, double centroZ)
        {
            double x = Math.Cos(angulo) * radioDeOrbita;
            double z = Math.Sin(angulo) * radioDeOrbita;

            return new vector(x, 0, centroZ + z);
        }

        void dibujarEsfera(Graphics g, vector position, Pen pen, double anguloRotacion, double escala = 1.0)
        {
            PointF[] points = new PointF[esferaVertices.Count];

            for (int i = 0; i < esferaVertices.Count; i++)
            {
                // 1. Aplicamos la escala al tamaño del planeta
                vector vEscalado = new vector(
                    esferaVertices[i].X * escala,
                    esferaVertices[i].Y * escala,
                    esferaVertices[i].Z * escala
                );

                // 2. Rotamos la esfera en el eje y
                vector vRotado = rotacion(vEscalado, anguloRotacion);

                // 3. Trasladamos la esfera a su posición final en el espacio
                vector traslacion = new vector(
                    vRotado.X + position.X,
                    vRotado.Y + position.Y,
                    vRotado.Z + position.Z
                );

                // 4. Proyectamos de 3D a 2D
                points[i] = Project(traslacion);
            }

            // Dibujamos las líneas uniendo los vértices proyectados
            foreach (int[] borde in esferaBordes)
            {
                int a = borde[0];
                int b = borde[1];
                g.DrawLine(pen, points[a], points[b]);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // 1. Dibujar el Sol en el centro (Desplazado en Z para que no esté pegado a la cámara)
            double zCentroSol = 600;
            vector centroSol = new vector(0, 0, zCentroSol);
            dibujarEsfera(g, centroSol, Pens.Yellow, anguloRotacionSol, 1); // Escala Sol = 1 (Radio 50)

            // 2. Definir los radios de órbita (Escala visual comprimida para caber en pantalla)
            double radioMercurio = 80;
            double radioVenus = 120;
            double radioTierra = 160;
            double radioMarte = 200;
            double radioJupiter = 300;
            double radioSaturno = 400;
            double radioUrano = 500;
            double radioNeptuno = 600;

            // 3. Calcular posiciones en la órbita (Aceleramos los interiores y frenamos los exteriores)
            vector Mercurio = CalcularPosicionOrbita(radioMercurio, anguloOrbita * 1.60, zCentroSol);
            vector Venus = CalcularPosicionOrbita(radioVenus, anguloOrbita * 1.17, zCentroSol);
            vector Tierra = CalcularPosicionOrbita(radioTierra, anguloOrbita * 1.00, zCentroSol);
            vector Marte = CalcularPosicionOrbita(radioMarte, anguloOrbita * 0.80, zCentroSol);
            vector Jupiter = CalcularPosicionOrbita(radioJupiter, anguloOrbita * 0.40, zCentroSol);
            vector Saturno = CalcularPosicionOrbita(radioSaturno, anguloOrbita * 0.30, zCentroSol);
            vector Urano = CalcularPosicionOrbita(radioUrano, anguloOrbita * 0.20, zCentroSol);
            vector Neptuno = CalcularPosicionOrbita(radioNeptuno, anguloOrbita * 0.10, zCentroSol);

            // 4. Dibujar los Planetas (Con la escala de tamaño real que calculamos antes)
            dibujarEsfera(g, Mercurio, Pens.DarkGray, anguloRotacionPlaneta, 0.175);
            dibujarEsfera(g, Venus, Pens.Coral, anguloRotacionPlaneta, 0.435);
            dibujarEsfera(g, Tierra, Pens.DeepSkyBlue, anguloRotacionPlaneta, 0.455);
            dibujarEsfera(g, Marte, Pens.OrangeRed, anguloRotacionPlaneta, 0.245);
            dibujarEsfera(g, Jupiter, Pens.Peru, anguloRotacionPlaneta, 5.02);
            dibujarEsfera(g, Saturno, Pens.Khaki, anguloRotacionPlaneta, 4.18);
            dibujarEsfera(g, Urano, Pens.Turquoise, anguloRotacionPlaneta, 1.82);
            dibujarEsfera(g, Neptuno, Pens.RoyalBlue, anguloRotacionPlaneta, 1.77);

            base.OnPaint(e);
        }
    }
}
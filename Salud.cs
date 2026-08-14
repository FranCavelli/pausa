using System;
using System.Collections.Generic;

namespace Pausa
{
    public class Ejercicio
    {
        public string Titulo;
        public string Detalle;
        public int Segundos;

        public Ejercicio(string titulo, string detalle, int segundos)
        {
            Titulo = titulo;
            Detalle = detalle;
            Segundos = segundos;
        }
    }

    // Contenido de las pausas. Los tiempos siguen la regla 20-20-20 (foco lejano cada 20
    // minutos) y el corte del sedentarismo cada 30 minutos, con una pausa larga por hora.
    public static class Salud
    {
        static readonly Random rnd = new Random();

        // Variantes de la micro pausa ocular: siempre foco lejano, cambia la forma
        static readonly string[][] Ocular = new string[][]
        {
            new string[] { "Mirá a lo lejos",
                "Buscá un punto a más de 6 metros: la ventana, el fondo del pasillo, un árbol.\nSostené la mirada ahí, sin forzar." },
            new string[] { "Lejos y cerca",
                "Alterná: 4 segundos mirando lo más lejano que veas, 4 segundos mirando tu pulgar\na un palmo de la cara. Repetí hasta que termine el conteo." },
            new string[] { "Parpadeo lento",
                "Mirá lejos y parpadeá completo y lento, 10 veces.\nFrente a la pantalla parpadeás la mitad de lo normal, y por eso arde el ojo." },
            new string[] { "Palmas sobre los ojos",
                "Frotá las palmas, apoyalas ahuecadas sobre los ojos cerrados sin apretar.\nOscuridad total, respiración tranquila." },
            new string[] { "Recorré el horizonte",
                "Sin mover la cabeza, llevá la mirada despacio a la izquierda, arriba,\nderecha y abajo. Terminá mirando el punto más lejano de la habitación." },
            new string[] { "Mirá por la ventana",
                "Si hay ventana, asomate. Foco en lo más lejano que encuentres:\nun edificio, una antena, una nube." }
        };

        // Micro pausas para cortar el sedentarismo (de pie, 20 s)
        static readonly string[][] DePie = new string[][]
        {
            new string[] { "Parate",
                "Levantate de la silla y estirá los brazos por encima de la cabeza.\nEstar sentado de corrido es lo que hace daño, no la silla." },
            new string[] { "Soltá los hombros",
                "De pie: subí los hombros hasta las orejas, sostené 3 segundos y soltalos de golpe.\nRepetí 5 veces." },
            new string[] { "Abrí el pecho",
                "Parate, entrelazá las manos atrás de la espalda y abrí el pecho.\nCompensa las horas encorvado hacia el teclado." },
            new string[] { "Caminá un poco",
                "Levantate y caminá hasta la otra punta del ambiente y volvé.\nAlcanza con moverse un minuto por cada media hora." },
            new string[] { "Cuello largo",
                "Meté un poco el mentón (doble mentón) y estirá la coronilla hacia el techo.\nSostené 10 segundos. Corrige la cabeza adelantada." }
        };

        // Rutina de la pausa larga. Se recorre en orden y se reparte en el tiempo configurado.
        static readonly Ejercicio[] Rutina = new Ejercicio[]
        {
            new Ejercicio("Levantate de la silla",
                "Parate, sacudí las piernas y los brazos.\nSi podés, alejate del escritorio hasta que termine la pausa.", 30),
            new Ejercicio("Cuello: lado a lado",
                "Llevá la oreja hacia el hombro, sin subir el hombro. 20 segundos de cada lado.\nSuave, nunca hasta el dolor.", 45),
            new Ejercicio("Hombros y espalda alta",
                "Círculos de hombros hacia atrás, 10 veces.\nDespués entrelazá las manos adelante y redondeá la espalda, empujando lejos.", 40),
            new Ejercicio("Muñecas y dedos",
                "Brazo estirado, palma arriba: con la otra mano llevá los dedos hacia vos.\n20 segundos por mano. Después abrí y cerrá los puños 10 veces.", 45),
            new Ejercicio("Cadera y piernas",
                "De pie, llevá una rodilla al pecho y sostené. 15 segundos por pierna.\nDespués 10 elevaciones de talones.", 40),
            new Ejercicio("Descanso ocular profundo",
                "Sentate derecho, cerrá los ojos y tapalos con las palmas ahuecadas.\nRespirá lento: 4 segundos entra, 6 segundos sale.", 45),
            new Ejercicio("Tomá agua",
                "Andá a buscar el vaso y tomá agua.\nLa deshidratación leve da dolor de cabeza y cansancio ocular.", 30),
            new Ejercicio("Foco lejano final",
                "Antes de volver: mirá lo más lejos que puedas durante todo el conteo.\nDejá los ojos enfocados afuera, no en la pantalla.", 45)
        };

        // Consejos de fondo, rotan en el pie del cartel
        static readonly string[] Consejos = new string[]
        {
            "La pantalla va a un brazo de distancia y con el borde superior a la altura de los ojos.",
            "El brillo de la pantalla debería parecerse al de la pared que tenés atrás.",
            "Los pies apoyados en el piso y las rodillas en ángulo recto: la zona lumbar lo agradece.",
            "Si al final del día te arden los ojos, casi siempre es falta de parpadeo, no falta de anteojos.",
            "El aire acondicionado apuntando a la cara reseca los ojos más rápido que la pantalla.",
            "Los codos cerca del cuerpo y las muñecas rectas evitan la mitad de las molestias del mouse.",
            "Cambiar de postura cada tanto vale más que encontrar la postura perfecta.",
            "La luz de la ventana conviene de costado: de frente encandila, de atrás hace reflejos.",
            "Si usás anteojos, la distancia a la pantalla no es la misma que la de lectura: decilo en el control.",
            "Cortar el tiempo sentado cada media hora baja el dolor lumbar aunque el total de horas no baje."
        };

        public static string[] MicroOcular() { return Ocular[rnd.Next(Ocular.Length)]; }
        public static string[] MicroDePie() { return DePie[rnd.Next(DePie.Length)]; }
        public static string Consejo() { return Consejos[rnd.Next(Consejos.Length)]; }

        // Arma la secuencia de la pausa larga ajustada a los segundos disponibles
        public static List<Ejercicio> RutinaLarga(int segundosTotales)
        {
            List<Ejercicio> lista = new List<Ejercicio>();
            int baseTotal = 0;
            foreach (Ejercicio e in Rutina) baseTotal += e.Segundos;

            int acumulado = 0;
            for (int i = 0; i < Rutina.Length; i++)
            {
                int seg = (int)Math.Round(Rutina[i].Segundos * (double)segundosTotales / baseTotal);
                if (seg < 10) seg = 10;
                if (i == Rutina.Length - 1) seg = Math.Max(10, segundosTotales - acumulado);
                if (acumulado >= segundosTotales) break;
                if (acumulado + seg > segundosTotales) seg = segundosTotales - acumulado;
                acumulado += seg;
                lista.Add(new Ejercicio(Rutina[i].Titulo, Rutina[i].Detalle, seg));
            }
            if (lista.Count == 0)
                lista.Add(new Ejercicio(Rutina[0].Titulo, Rutina[0].Detalle, segundosTotales));
            return lista;
        }
    }
}

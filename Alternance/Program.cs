using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Alternance
{
    class Program
    {
        //---------- VARIABLES ----------//
        static int timer = 0;
        static Mutex Alternance = new Mutex();
        static int delaiMaster = 3;
        static int delaiSlave = 3;
        static int tour = 0;
        static bool traiterMaster = false;
        static bool traiterSlave = false;
        static Mutex MutexTimer = new Mutex();
        static Thread slave;
        static bool slaveDemarrer = false;
        static int tempsNavigation = 60;
        static TextWriter tx = new StreamWriter("log.txt", true);


        static void Main(string[] args)
        {
            try
            {
                Console.Write("Entrez le temps d'intervalles du thread slave: ");
                string line = Console.ReadLine();
                while (!int.TryParse(line, out delaiSlave))
                {
                    Console.Write("Entrez un chiffre pour le delai: ");
                    line = Console.ReadLine();
                }
                tx.WriteLine("Entrez un chiffre pour le delai: " + line);

                Console.Write("Entrez le temps de navigation: ");
                line = Console.ReadLine();
                while (!int.TryParse(line, out tempsNavigation))
                {
                    Console.Write("Entrez un chiffre pour le temps de navigation: ");
                    line = Console.ReadLine();
                }
                tx.WriteLine("Entrez le temps de navigation: " + line);
                tx.WriteLine("--------------------------------------");

                new Thread(Timer).Start();
                Thread master = new Thread(Master);
                master.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Timer()
        {
            while (true)
            {
                Thread.Sleep(1000);
                MutexTimer.WaitOne();
                timer++;
                traiterMaster = false;
                traiterSlave = false;
                MutexTimer.ReleaseMutex();
            }
        }

        static void Master()
        {
            Alternance.WaitOne();
            while (timer != tempsNavigation)
            {
                MutexTimer.WaitOne();
                bool traiter = traiterMaster;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();
                if (timerlocal == 5 && !slaveDemarrer)
                {
                    slave = new Thread(Slave);
                    slave.Start();
                    slaveDemarrer = true;
                }

                if ((timerlocal % delaiMaster) == 0 && estEnConflit() && !traiter)
                {
                    if (tour % 2 != 0)
                    {
                        Alternance.WaitOne();

                        tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et ecris");

                        //Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                        tour++;
                        traiterMaster = true;
                    }
                    else
                    {
                        tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Alternance.ReleaseMutex();

                        traiterMaster = true;
                    }

                }
                else if ((timerlocal % delaiMaster) == 0 && !traiter)
                {
                    tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes");
                    Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes");
                    traiterMaster = true;
                }
            }
            while (slave.IsAlive)
            {
                // Attend que le Slave Thread meurt avant de tuer le Master Thread
            }
            tx.WriteLine("Master Thread : terminé");
            Console.WriteLine("Master Thread : terminé");
            tx.WriteLine("Fin du programme");
            Console.WriteLine("Fin du programme");
            tx.WriteLine("--------------------------------------");
            tx.WriteLine("--------------------------------------");
            tx.Close();
        }

        static void Slave()
        {
            tx.WriteLine("Création du Slave Thread");
            Console.WriteLine("Création du Slave Thread");
            while (timer != tempsNavigation)
            {
                MutexTimer.WaitOne();
                bool traiter = traiterSlave;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();

                if ((timerlocal % delaiSlave) == 0 && estEnConflit() && !traiter)
                {
                    if (tour % 2 == 0)
                    {
                        Alternance.WaitOne();
                        tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                        tour++;
                        traiterSlave = true;
                    }
                    else
                    {
                        tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Alternance.ReleaseMutex();
                        traiterSlave = true;
                    }
                }
                else if ((timerlocal % delaiSlave) == 0 && !traiter)
                {
                    tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes");
                    Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes");
                    traiterSlave = true;
                }
            }
            tx.WriteLine("Slave Thread : terminé");
            Console.WriteLine("Slave Thread : terminé");
        }
        static bool estEnConflit()
        {
            return ((timer % delaiMaster) == 0) && ((timer % delaiSlave) == 0) && slaveDemarrer;
        }
    }
}
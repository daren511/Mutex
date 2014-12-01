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
        // Int
        static int timer = 0;
        static int delaiMaster = 3;
        static int delaiSlave = 3;
        static int tour = 0;
        static int tempsCreationSlave = 5;
        // Bool
        static bool traiterMaster = false;
        static bool traiterSlave = false;
        static bool slaveDemarrer = false;
        // Mutex
        static Mutex Alternance = new Mutex();
        static Mutex MutexTimer = new Mutex();
        // Thread
        static Thread master;
        static Thread slave;
        // Textwriter
        static TextWriter tx = new StreamWriter("log.txt", true);


        static void Main(string[] args)
        {
            try
            {
                // Entrée du temps d'intervalles pour le Slave Thread dans la console
                Console.Write("Entrez le temps d'intervalles du thread slave: ");
                string line = Console.ReadLine();
                while (!int.TryParse(line, out delaiSlave))
                {
                    Console.Write("Entrez un chiffre pour le delai: ");
                    line = Console.ReadLine();
                }
                tx.WriteLine("Entrez un chiffre pour le delai: " + line);

                // Entrée du temps d'attente pour la création du Slave Thread dans la console
                Console.Write("Entrez le temps d'attente de création du slave: ");
                line = Console.ReadLine();
                while (!int.TryParse(line, out tempsCreationSlave))
                {
                    Console.Write("Entrez un chiffre pour le temps de création: ");
                    line = Console.ReadLine();
                }
                tx.WriteLine("Entrez le temps de navigation: " + line);
                tx.WriteLine("--------------------------------------");

                // Début du Timer
                new Thread(Timer).Start();

                // Début du Master Thread
                master = new Thread(Master);
                master.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Timer qui compte les secondes utilisées par les deux Threads
        /// </summary>
        static void Timer()
        {
            while (true)
            {
                Thread.Sleep(1000);
                MutexTimer.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal 
                timer++;
                traiterMaster = false;
                traiterSlave = false;
                MutexTimer.ReleaseMutex();  // Libère le mutex
            }
        }

        /// <summary>
        /// Effectue l'écriture et tout autre traitement pour le Master Thread
        /// </summary>
        static void Master()
        {
            Alternance.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal
            while (timer != 30)
            {
                MutexTimer.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal
                bool traiter = traiterMaster;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();  // Libère le Mutex

                // Si c'est maintenant le temps que l'usager a entré dans la console
                if (timerlocal == tempsCreationSlave && !slaveDemarrer)
                {
                    // Crée et démarre le Slave Thread
                    slave = new Thread(Slave);
                    slave.Start();
                    slaveDemarrer = true;
                }

                // S'il y a un conflit entre les 2 Threads
                if ((timerlocal % delaiMaster) == 0 && estEnConflit() && !traiter)
                {
                    // Si le tour est impair
                    if (tour % 2 != 0)
                    {
                        Alternance.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal 
                        // Écriture dans la console et le fichier texte
                        tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et écris");
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et écris");

                        tour++;
                        traiterMaster = true;
                    }
                    else
                    {
                        // Écriture dans la console et le fichier texte
                        tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes écris et release ");
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes écris et release ");

                        Alternance.ReleaseMutex();
                        traiterMaster = true;
                    }
                }
                // Si le thread n'est pas traité et que le délai est un diviseur du timer
                else if ((timerlocal % delaiMaster) == 0 && !traiter)
                {
                    // Écriture dans la console et le fichier texte
                    tx.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes");
                    Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes");
                    traiterMaster = true;
                }
            }

            while (slave.IsAlive)
            {
                // Attendre que le Slave Thread se termine
            }

            // Écriture dans la console et le fichier texte
            tx.WriteLine("Master Thread : terminé");
            Console.WriteLine("Master Thread : terminé");

            tx.WriteLine("Fin du programme");
            Console.WriteLine("Fin du programme");

            tx.WriteLine("--------------------------------------");
            tx.WriteLine("--------------------------------------");
            // Fermeture du TextWriter
            tx.Close();
        }

        /// <summary>
        /// Effectue l'écriture et tout autre traitement pour le Slave Thread
        /// </summary>
        static void Slave()
        {
            // Écriture dans la console et le fichier texte
            tx.WriteLine("Création du Slave Thread");
            Console.WriteLine("Création du Slave Thread");
            while (timer != 30)
            {
                MutexTimer.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal 
                bool traiter = traiterSlave;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();  // Libère le Mutex

                // S'il y a un conflit entre les 2 Threads
                if ((timerlocal % delaiSlave) == 0 && estEnConflit() && !traiter)
                {
                    // Si le tour est impair
                    if (tour % 2 == 0)
                    {
                        Alternance.WaitOne();   // Bloque juqu'à ce qu'il reçoit un signal 
                        // Écriture dans la console et le fichier texte
                        tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes pris et écris");
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes pris et écris");

                        tour++;
                        traiterSlave = true;
                    }
                    else
                    {
                        // Écriture dans la console et le fichier texte
                        tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes écris et release ");
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes écris et release ");
                        Alternance.ReleaseMutex();
                        traiterSlave = true;
                    }
                }
                // Si le thread n'est pas traité et que le délai est un diviseur du timer
                else if ((timerlocal % delaiSlave) == 0 && !traiter)
                {
                    // Écriture dans la console et le fichier texte
                    tx.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes");
                    Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes");
                    traiterSlave = true;
                }
                // Tue le Thread si le Master Thread est mort
                if (!master.IsAlive)
                    slave.Abort();
            }
            // Écriture dans la console et le fichier texte
            tx.WriteLine("Slave Thread : terminé");
            Console.WriteLine("Slave Thread : terminé");
        }

        /// <summary>
        /// Vérifie s'il y a un conflit entre les 2 Threads
        /// </summary>
        /// <returns>Retourne true s'il n'y a aucun conflit</returns>
        static bool estEnConflit()
        {
            return ((timer % delaiMaster) == 0) && ((timer % delaiSlave) == 0) && slaveDemarrer;
        }
    }
}
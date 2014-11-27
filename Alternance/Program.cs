using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Alternance
{
    class Program
    {
        static int timer = 0;
        static Mutex Alternance = new Mutex();
        static int delaiMaster = 3;
        static int delaiSlave = 3;
        static int tour = 0;
        static bool traiterMaster = false;
        static bool traiterSlave = false;
        static Mutex MutexTimer = new Mutex();
        static void Main(string[] args)
        {
            try
            {
                new Thread(Timer).Start();
                Thread master = new Thread(Master);
                Thread slave = new Thread(Slave);
                
                master.Start();
                System.Threading.Thread.Sleep(15);
                slave.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Timer()
        {
            while(true)
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
            while (timer != 60)
            {
                MutexTimer.WaitOne();
                bool traiter = traiterMaster;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();
                if ((timerlocal % delaiMaster) == 0 && estEnConflit() && !traiter)
                {
                    if (tour % 2 != 0)
                    {
                        Alternance.WaitOne();
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                        tour++;
                        traiterMaster = true;
                    }
                    else
                    {
                      
                        Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Alternance.ReleaseMutex();
                        
                        traiterMaster = true;                        
                    }
                   
                }
                else if ((timerlocal % delaiMaster) == 0 && !traiter)
                {
                    Console.WriteLine("Master Thread : Temps passé " + timerlocal + " secondes");
                    traiterMaster = true;
                }
                
            }
        }

        static void Slave()
        {
            
            while (timer != 60)
            {
                MutexTimer.WaitOne();
                bool traiter = traiterSlave;
                int timerlocal = timer;
                MutexTimer.ReleaseMutex();

                if ((timerlocal % delaiSlave) == 0 && estEnConflit() && !traiter)
                {
                    if (tour % 2 == 0 )
                    {
                        Alternance.WaitOne();
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes pris et ecris");
                         tour++;
                        traiterSlave = true;
                       
                    }
                    else
                    {
                       
                        Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes ecris et release ");
                        Alternance.ReleaseMutex();
                        traiterSlave = true;
                    }
                }
                else if ((timerlocal % delaiSlave) == 0 && !traiter)
                {
                    Console.WriteLine("Slave Thread : Temps passé " + timerlocal + " secondes");
                    traiterSlave = true;
                }
                
            }
        }
        static bool estEnConflit()
        {
            return ((timer % delaiMaster) == 0) && ((timer % delaiSlave) == 0);
        }
    }
}

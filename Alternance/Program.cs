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
        const int TIMER_LIMIT = 20;
        const int SLAVE_START_TIME = 5;
        static int timer = 0;
        static Mutex Alternance = new Mutex();
        static bool tourMaster = true;
        static int delaiMaster = 3;
        static int delaiSlave = 2;
        static bool traiterMaster = false;
        static bool traiterSlave = false; 
        static Mutex m = new Mutex();
        static Thread master = new Thread(Master);
        static Thread slave = new Thread(Slave);
        static void Main(string[] args)
        {
            new Thread(Timer).Start();

            master.Start();
        }

        static void Timer()
        {
            while(true)
            {
                Thread.Sleep(1000);
                timer++;
                 traiterMaster = false;
                 traiterSlave = false;
            }
        }

        static void Master()
        {

            while (timer != TIMER_LIMIT)
            {
                if (timer == SLAVE_START_TIME && !slave.IsAlive)
                {
                    slave.Start();
                }
                m.WaitOne();
                if ((timer % delaiMaster) == 0 && estEnConflit() && !traiterMaster)
                {
                    Alternance.WaitOne();
                    if (tourMaster)
                    {
                        
                        Console.WriteLine("Master Thread : Temps passé " + timer + " secondes CONFLIT");
                        Alternance.ReleaseMutex();
                        tourMaster = false;
                    }
                    else
                    {
                        Alternance.ReleaseMutex();
                        Alternance.WaitOne();
                        Console.WriteLine("Master Thread : Temps passé " + timer + " secondes CONFLIT");
                        Alternance.ReleaseMutex();
                    }
                    traiterMaster = true;
                }
                else if ((timer % delaiMaster) == 0 && !traiterMaster)
                {
                    Console.WriteLine("Master Thread : Temps passé " + timer + " secondes");
                    traiterMaster = true;
                }
                m.ReleaseMutex();
            }
        }

        static void Slave()
        {
            while (timer != TIMER_LIMIT)
            {
                m.WaitOne();
                if ((timer % delaiSlave) == 0 && estEnConflit() && !traiterSlave)
                {
                    if (tourMaster)
                    {
                        Alternance.WaitOne();
                        Console.WriteLine("Slave Thread : Temps passé " + timer + " secondes CONFLIT");
                        Alternance.ReleaseMutex();
                    }
                    else
                    {
                        Alternance.WaitOne();
                        Console.WriteLine("Slave Thread : Temps passé " + timer + " secondes CONFLIT");
                        Alternance.ReleaseMutex();
                        tourMaster = true;
                    }
                    traiterSlave = true;
                }
                else if ((timer % delaiSlave) == 0 && !traiterSlave)
                {
                    Console.WriteLine("Slave Thread : Temps passé " + timer + " secondes");
                    traiterSlave = true;
                }
                m.ReleaseMutex();
            }
        }
        static bool estEnConflit()
        {
            return ((timer % delaiMaster) == 0) && ((timer % delaiSlave) == 0);
        }
    }
}

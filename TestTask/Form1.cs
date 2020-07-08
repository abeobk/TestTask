using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTask {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
        }

        Task pollTask = null;
        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            should_stop = true;
            pollTask?.Wait();
            Debug.WriteLine("Task exit");
            Debug.WriteLine("Form destroyed");
        }


        ~Form1() {
        }

        Queue<string> msgqueue = new Queue<string>();
        Stopwatch swqueue = new Stopwatch();
        void Log(string msg) {
            var action = new Action(delegate {
                msgqueue.Enqueue(msg);
                //    if(swqueue.ElapsedMilliseconds)
                //        listBox1.Items.Add(msg);
                //    int visibleItems = listBox1.ClientSize.Height / listBox1.ItemHeight;
                //    listBox1.TopIndex = Math.Max(listBox1.Items.Count - visibleItems + 1, 0);
                //    this.Update();
                label1.Text = "msg_cnt: " + msgqueue.Count;
            });
            if(InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }


        bool started = false;
        bool should_stop = false;
        Stopwatch sw = new Stopwatch();

        private void button1_Click(object sender, EventArgs e) {
            if(pollTask !=null && pollTask.Status == TaskStatus.Running) {
                should_stop = true;
                return;
            }

            swqueue.Restart();
            timer1.Start();
            should_stop = false;
            //dispose it first
            pollTask?.Dispose();

            pollTask = Task.Run(async delegate {
                int i = 0;
                Log("Started");
                sw.Restart();
                double next_t = 0;
                double t = 0;
                for(; ; ) {
                    while((t = sw.ElapsedMilliseconds) < next_t) {
                        Thread.Sleep(1);
                    }
                    next_t += 1000;
                    //t = sw.Elapsed();
                    //var td = Task.Delay(1);

                    //test if program should stop
                    if(should_stop) {
                        Debug.WriteLine("Stopping...");
                        break;
                    }
                    Log("Time: \n\n" + t);

                    //await Task.FromResult(0);
                    //await td;
                }
                Log("Stopped");
                Debug.WriteLine("Stopped");
                started = false;
            });
            started = true;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if(msgqueue.Count == 0)
                return;
            foreach(var msg in msgqueue) {
                listBox1.Items.Add(msg);
            }
            msgqueue.Clear();
            int visibleItems = listBox1.ClientSize.Height / listBox1.ItemHeight;
            listBox1.TopIndex = Math.Max(listBox1.Items.Count - visibleItems + 1, 0);
            this.Update();
        }
    }
}

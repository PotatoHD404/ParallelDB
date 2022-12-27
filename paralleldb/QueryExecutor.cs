namespace ParallelDB;

// this class is used to execute the query plan in parallel C#
// public class QueryExecutor
// {
//     private QueryPlan _plan;
//     private int _numThreads;
//     private int _numTasks;
//     
//     public QueryExecutor(QueryPlan plan, int numThreads)
//     {
//         this.plan = plan;
//         this.numThreads = numThreads;
//     }
//     
//     public void Execute()
//     {
//         // create the threads
//         Thread[] threads = new Thread[numThreads];
//         for (int i = 0; i < numThreads; i++)
//         {
//             threads[i] = new Thread(new ThreadStart(ExecutePlan));
//         }
//     
//         // start the threads
//         for (int i = 0; i < numThreads; i++)
//         {
//             threads[i].Start();
//         }
//     
//         // wait for the threads to finish
//         for (int i = 0; i < numThreads; i++)
//         {
//             threads[i].Join();
//         }
//     }
// }
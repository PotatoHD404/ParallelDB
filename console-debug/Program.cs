// See https://aka.ms/new-console-template for more information

using System;
using ParallelDB;

var test = new Test();

Console.WriteLine(test.GetId()());

Console.WriteLine(test.SetId()(5));

Console.WriteLine(test.GetId()());
// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using Antlr4.Runtime;
using ConsoleTables;
using ParallelDB;
using SqlParser;
using Parser;


// var a =
//     @"SELECT a.doc as 'Врач', a.score as 'Оценка' FROM (SELECT doctor.doctor_id, doctor.doctor_name as doc, AVG(questionnaire.service_assessment) as score FROM doctor
// LEFT JOIN questionnaire ON doctor.doctor_id = questionnaire.doctor_id
// GROUP BY doctor.doctor_id, doctor.doctor_name
// ORDER BY AVG(questionnaire.service_assessment) ASC
// LIMIT 2) as a";
// var a = @"SELECT a.doc as 'Врач' FROM doctors as a WHERE a = b AND c = d OR c = f GROUP by c, d HAVING f = g ORDER BY a.name LIMIT 10 OFFSET 10;";

// Stopwatch sw = new Stopwatch();
// sw.Start();
// ICharStream stream = CharStreams.fromString(a);
// var lexer = new SQLiteLexer(stream);
// lexer.RemoveErrorListeners();
// lexer.AddErrorListener(new LexerErrorListener());
// var tokens = new CommonTokenStream(lexer);
// var parser = new SQLiteParser(tokens);
// parser.RemoveErrorListeners();
// parser.AddErrorListener(new ParserErrorListener());
// var tree = parser.parse();
// GraphvizVisitor graphvizVisitor = new();
// // SqlNodeVisitor visitor = new();
// // foreach tree.children
// // foreach(var child in tree.children)
// // {
// //     visitor.Visit(child);
// // }
// graphvizVisitor.Visit(tree);
// Console.WriteLine(graphvizVisitor.GetGraph());

var result = new Table("doc");
result.AddColumn("Врач", typeof(int), false);
result.AddColumn("Оценка", typeof(int));


// Console.WriteLine(result.Name);
// Console.WriteLine(result.RowCount);
//
// Console.WriteLine(result.ColumnCount);
// object? o1 = 1;

// object? o2 = 2;

// object? o3 = (dynamic)o1 + (dynamic)o2;
// Console.WriteLine(o3);


var row = result.NewRow();
Console.WriteLine("check");
row["Врач"] = 123;
//
row["Оценка"] = 2;

result.NewRow();
Func<TableRow, dynamic> t = el => el["Врач"] + el["Оценка"];


// Console.WriteLine(PrettyPrint.ToString(result.Select(el =>
// {
//     return new TableRow(el["Врач"] + el["Оценка"], el["Оценка"]);
// }).Where(el => el["Врач"] > 0).ToList()));
// row = result.NewRow();
// var list = result.ToList();
//
//
// // Console.WriteLine(list.Count);
//
// // print the results
//
// foreach (var item in list)
// {
//     Console.WriteLine(item);
// }
// result.RowCount;

// sw.Stop();

// Console.WriteLine(sw.ElapsedMilliseconds);
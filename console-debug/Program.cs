// See https://aka.ms/new-console-template for more information

using Antlr4.Runtime;
using SqlParser;
using Parser;


// var a =
//     @"SELECT a.doc as 'Врач', a.score as 'Оценка' FROM (SELECT doctor.doctor_id, doctor.doctor_name as doc, AVG(questionnaire.service_assessment) as score FROM doctor
// LEFT JOIN questionnaire ON doctor.doctor_id = questionnaire.doctor_id
// GROUP BY doctor.doctor_id, doctor.doctor_name
// ORDER BY AVG(questionnaire.service_assessment) ASC
// LIMIT 2) as a";
// var a = @"SELECT a.doc as 'Врач' FROM doctors as a WHERE a = b AND c = d OR c = f GROUP by c, d HAVING f = g ORDER BY a.name LIMIT 10 OFFSET 10;";
//
//
//
// // Stopwatch sw = new Stopwatch();
// // sw.Start();
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
// // Console.WriteLine(graphvizVisitor.GetGraph());

var result = new Table("doc");
result.AddColumn("Врач", typeof(int));
result.AddColumn("Оценка", typeof(int));

// Console.WriteLine(result.Name);
// Console.WriteLine(result.RowCount);
//
// Console.WriteLine(result.ColumnCount);

var row = result.NewRow();

row["Врач"] = 1;
row["Оценка"] = 2;
row = result.NewRow();
var list = result.ToList();

// Console.WriteLine(list.Count);

// print the results

foreach (var item in list)

{
    Console.WriteLine(item);
}
// result.RowCount;

// sw.Stop();

// Console.WriteLine(sw.ElapsedMilliseconds);
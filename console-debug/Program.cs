// See https://aka.ms/new-console-template for more information

using Antlr4.Runtime;
using System.Diagnostics;
using SqlParser;
using Parser;


// var a =
//     @"SELECT a.doc as 'Врач', a.score as 'Оценка' FROM (SELECT doctor.doctor_id, doctor.doctor_name as doc, AVG(questionnaire.service_assessment) as score FROM doctor
// LEFT JOIN questionnaire ON doctor.doctor_id = questionnaire.doctor_id
// GROUP BY doctor.doctor_id, doctor.doctor_name
// ORDER BY AVG(questionnaire.service_assessment) ASC
// LIMIT 2) as a";
var a = @"SELECT a.doc as 'Врач' FROM doctors as a";

// Stopwatch sw = new Stopwatch();
// sw.Start();
ICharStream stream = CharStreams.fromString(a);
var lexer = new SQLiteLexer(stream);
var tokens = new CommonTokenStream(lexer);
var parser = new SQLiteParser(tokens);
var tree = parser.parse();
ParallelDbVisitor visitor = new ParallelDbVisitor();
visitor.Visit(tree);
// sw.Stop();

// Console.WriteLine(sw.ElapsedMilliseconds);


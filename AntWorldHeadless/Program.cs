
var antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(256, 1, 32, 64, 256);


var ctr = 0;
foreach (var aw in antWorldSeq)
{
    ctr++;
    if (ctr % 1000 == 0) Console.WriteLine(ctr);
}
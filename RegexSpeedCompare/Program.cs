namespace RegexSpeedCompare
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            RunSpeedTest();
        }

        private static void RunSpeedTest()
        {
            var loopCount = 1000000;
            var sourceText = @"
CREATE TABLE dbo.AttributeType(
	Id int IDENTITY(11) NOT NULL,
	Name nvarchar(50) NOT NULL,
	CONSTRAINT PK_AttributeType PRIMARY KEY CLUSTERED (Id),
);

CREATE TABLE dbo.Attribute(
	Id int IDENTITY(11) NOT NULL,
	Name nvarchar(50) NOT NULL,
	Description nvarchar(max) NULL,
	AttributeTypeId int NOT NULL,
	CONSTRAINT PK_Attribute PRIMARY KEY CLUSTERED (Id),
	CONSTRAINT FK_Attribute_AttributeType_AttributeTypeId FOREIGN KEY (AttributeTypeId) REFERENCES dbo.AttributeType(Id),
);

CREATE TABLE dbo.Person(
	Id int IDENTITY(11) NOT NULL,
	GivenName nvarchar(50) NOT NULL,
	FamilyName nvarchar(50) NOT NULL,
	CONSTRAINT PK_Person PRIMARY KEY CLUSTERED (Id),
);

CREATE TABLE dbo.PersonAttribute(
	Id int IDENTITY(11) NOT NULL,
	PersonId int NOT NULL,
	AttributeId int NOT NULL,
	CONSTRAINT PK_PersonAttribute PRIMARY KEY CLUSTERED (Id),
	CONSTRAINT FK_PersonAttribute_Person_PersonId FOREIGN KEY (PersonId) REFERENCES dbo.Person(Id),
	CONSTRAINT FK_PersonAttribute_Attribute_AttributeId FOREIGN KEY (AttributeId) REFERENCES dbo.Attribute(Id),
);";
            var singleStep = new Tuple<Regex, string>(
                new Regex(
                    @"(?:\s+)?CREATE TABLE (\w+(?:\.\w+)?)\([^;]*;(?:\s+)?",
                    RegexOptions.Compiled | RegexOptions.Multiline),
                "DROP TABLE $1;\r\n");

            var multipleSteps = new List<Tuple<Regex, string>>
                {
                    new Tuple<Regex, string>(
                        new Regex(@"CREATE TABLE", RegexOptions.Compiled | RegexOptions.Multiline),
                        "DROP TABLE"),
                    new Tuple<Regex, string>(
                        new Regex(@"\([^;]*\)", RegexOptions.Compiled | RegexOptions.Multiline),
                        string.Empty),
                    new Tuple<Regex, string>(
                        new Regex(@"^\s*", RegexOptions.Compiled | RegexOptions.Multiline),
                        string.Empty),
                    new Tuple<Regex, string>(
                        new Regex(@"\s*$", RegexOptions.Compiled | RegexOptions.Multiline),
                        string.Empty),
                    new Tuple<Regex, string>(
                        new Regex(@"^\s*$\r\n", RegexOptions.Compiled | RegexOptions.Multiline),
                        string.Empty)
                };

            var stopwatchSingle = new Stopwatch();
            stopwatchSingle.Start();
            for (var i = 0; i < loopCount; i++)
            {
                var loopText = sourceText;
                var singleResult = singleStep.Item1.Replace(loopText, singleStep.Item2);
            }

            stopwatchSingle.Stop();
            Console.WriteLine("Elapsed time for single step loop: {0}", stopwatchSingle.ElapsedMilliseconds);

            var stopwatchStep = new Stopwatch();
            stopwatchStep.Start();
            for (var i = 0; i < loopCount; i++)
            {
                var stepText = sourceText;
                foreach (var step in multipleSteps)
                {
                    stepText = step.Item1.Replace(stepText, step.Item2);
                }
            }

            stopwatchStep.Stop();
            Console.WriteLine("Elapsed time for single step loop: {0}", stopwatchStep.ElapsedMilliseconds);
            Console.WriteLine("Test done!");

            Console.ReadLine();
        }
    }
}

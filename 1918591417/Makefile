Reference := /home/logan/rwreference-1.3
Assemblies/MeatFix.dll: Source/*.cs $(Reference)/Assembly-CSharp.dll
	csc -target:library -out:Assemblies/MeatFix.dll -r:$(Reference)/Assembly-CSharp.dll -r:$(Reference)/UnityEngine.dll -r:$(Reference)/HugsLib.dll -r:$(Reference)/0Harmony.dll Source/*.cs -r:$(Reference)/System.Configuration.dll -r:$(Reference)/System.Security.dll
all: Assemblies/MeatFix.dll

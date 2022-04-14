
.PHONY: clean

step1_read_print: step1-build/step1
	cp step1-build/step1 step1_read_print

step1-build/step1: step1/*.cs step1/step1.csproj
	dotnet publish step1/step1.csproj --configuration Release --nologo --output step1-build --runtime linux-x64 --self-contained

step1_repl: step1-build/step1
	cp step0-build/step0 step0_repl

step0-build/step0: step0/*.cs step0/step0.csproj
	dotnet publish step0/step0.csproj --configuration Release --nologo --output step0-build --runtime linux-x64 --self-contained

clean:
	rm -r step?/obj step?/bin step?-build
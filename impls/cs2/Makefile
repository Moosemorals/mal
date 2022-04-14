
.PHONY: clean

step0_repl: step0-build/step0
	cp step0-build/step0 step0_repl

step0-build/step0: step0/*.cs step0/step0.csproj
	dotnet publish step0/step0.csproj --configuration Release --nologo --output step0-build --runtime linux-x64 --self-contained


clean:
	rm -r step?/obj step?/bin step?-build
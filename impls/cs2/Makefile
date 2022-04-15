
.PHONY: clean

step4_if_fn_do: step4-build/step4
	cp step4-build/step4 step4_if_fn_do

step4-build/step4: step4/*.cs step4/step4.csproj
	dotnet publish step4/step4.csproj --configuration Release --nologo --output step4-build --runtime linux-x64 --self-contained



step3_env: step3-build/step3
	cp step3-build/step3 step3_env

step3-build/step3: step3/*.cs step3/step3.csproj
	dotnet publish step3/step3.csproj --configuration Release --nologo --output step3-build --runtime linux-x64 --self-contained


step2_eval: step2-build/step2
	cp step2-build/step2 step2_eval

step2-build/step2: step2/*.cs step2/step2.csproj
	dotnet publish step2/step2.csproj --configuration Release --nologo --output step2-build --runtime linux-x64 --self-contained


step1_read_print: step1-build/step1
	cp step1-build/step1 step1_read_print

step1-build/step1: step1/*.cs step1/step1.csproj
	dotnet publish step1/step1.csproj --configuration Release --nologo --output step1-build --runtime linux-x64 --self-contained



step0_repl: step0-build/step0
	cp step0-build/step0 step0_repl

step0-build/step0: step0/*.cs step0/step0.csproj
	dotnet publish step0/step0.csproj --configuration Release --nologo --output step0-build --runtime linux-x64 --self-contained

clean:
	rm -r step?/obj step?/bin step?-build

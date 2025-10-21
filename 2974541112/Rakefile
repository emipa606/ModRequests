require "rimtool/rake_tasks"

desc "release"
task :release => [:mod, :plugins]

# compile plugins before running tests
Rake::Task[:test].enhance [:plugins]

task :plugins do
  Dir.chdir "Source/Plugins"
  Dir["*.csproj"].each do |fname|
    system "dotnet build -c Release #{fname}", exception: true
  end
  Dir.chdir "../.."
end

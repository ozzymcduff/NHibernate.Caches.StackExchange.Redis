
require 'albacore'
require 'nuget_helper'

task :default => ['build']

desc "build"
build :build do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Any CPU"
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln =File.join('.', "src", "NHibernate.Caches.StackExchange.Redis.sln")
end

build :build_release do |msb|
  msb.prop :configuration, :Release
  msb.prop :platform, "Any CPU"
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln =File.join('.', "src", "NHibernate.Caches.StackExchange.Redis.sln")
end

desc "test using console"
test_runner :test => [:build] do |runner|
  runner.exe = NugetHelper::xunit_path
  files = [File.join(".","src","NHibernate.Caches.StackExchange.Redis.Tests","bin","Debug",
    "NHibernate.Caches.StackExchange.Redis.Tests.dll")]
  runner.files = files 
end


desc "Install missing NuGet packages."
task :restore do
  NugetHelper::exec("restore ./src/NHibernate.Caches.StackExchange.Redis.sln")
end

desc "create the nuget package"
task :pack => [:build_release] do |nuget|
  cd File.join('.', "src", "NHibernate.Caches.StackExchange.Redis") do
    NugetHelper.exec "pack NHibernate.Caches.StackExchange.Redis.csproj"
  end
end

def get_last_version(files)
  files.max_by do |l|
     NugetHelper.version_of(l)
  end
end

desc "Push to NuGet."
task :push do
  latest = get_last_version(Dir.glob(File.join('.', "src", "NHibernate.Caches.StackExchange.Redis", '*.nupkg')))
  NugetHelper.exec("push #{latest}")
end

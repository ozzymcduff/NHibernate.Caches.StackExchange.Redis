
require 'albacore'
require_relative './src/.nuget/nuget'

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

desc "test using console"
test_runner :test => [:build] do |runner|
  runner.exe = NuGet::nunit_path
  files = [File.join(File.dirname(__FILE__),"src","NHibernate.Caches.StackExchange.Redis.Tests","bin","Debug",
    "NHibernate.Caches.StackExchange.Redis.Tests.dll")]
  runner.files = files 
end


desc "Install missing NuGet packages."
task :install_packages do
  package_paths = FileList["src/**/packages.config"]+["src/.nuget/packages.config"]

  package_paths.each.each do |filepath|
      NuGet::exec("i #{filepath} -o ./src/packages -source http://www.nuget.org/api/v2/")
  end
end
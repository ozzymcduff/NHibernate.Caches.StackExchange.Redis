
require 'albacore'
require 'rbconfig'
#http://stackoverflow.com/questions/11784109/detecting-operating-systems-in-ruby
def os
  @os ||= (
    host_os = RbConfig::CONFIG['host_os']
    case host_os
    when /mswin|msys|mingw|cygwin|bccwin|wince|emc/
      :windows
    when /darwin|mac os/
      :macosx
    when /linux/
      :linux
    when /solaris|bsd/
      :unix
    else
      raise Error::WebDriverError, "unknown os: #{host_os.inspect}"
    end
  )
end

def nuget_exec(parameters)

  command = File.join(File.dirname(__FILE__), "src",".nuget","NuGet.exe")
  if os == :windows
    sh "#{command} #{parameters}"
  else
    sh "mono --runtime=v4.0.30319 #{command} #{parameters} "
  end
end

task :default => ['build']

def nunit_cmd()
  cmds = Dir.glob(File.join(File.dirname(__FILE__),"src","packages","NUnit.Runners.*","tools","nunit-console.exe"))
  if cmds.any?
    if os != :windows
      command = "mono --runtime=v4.0.30319 #{cmds.first}"
    else
      command = cmds.first
    end
  else
    raise "Could not find nunit runner!"
  end
  return command
  
end

def nunit_exec(dir, tlib)
    command = nunit_cmd()
    assemblies= "#{tlib}.dll"
    cd dir do
      sh "#{command} #{assemblies}" do  |ok, res|
        if !ok
          abort 'Nunit failed!'
        end
      end
    end

end

def with_mono_properties msb
  solution_dir = File.join(File.dirname(__FILE__),'src')
  nuget_tools_path = File.join(solution_dir, '.nuget')
  msb.prop :SolutionDir, solution_dir
  msb.prop :NuGetToolsPath, nuget_tools_path
  msb.prop :NuGetExePath, File.join(nuget_tools_path, 'NuGet.exe')
  msb.prop :PackagesDir, File.join(solution_dir, 'packages')
end

desc "build"
build :build do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Any CPU"
  if os != :windows
    with_mono_properties msb
  end
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln =File.join('.', "src", "NHibernate.Caches.StackExchange.Redis.sln")
end

desc "test using nunit console"
task :test => :build do |t|
  dir = File.join('.',"src","NHibernate.Caches.StackExchange.Redis.Tests","bin","Debug")
  nunit_exec(dir,"NHibernate.Caches.StackExchange.Redis.Tests")
end

desc "Install missing NuGet packages."
task :install_packages do
  package_paths = FileList["src/**/packages.config"]+["src/.nuget/packages.config"]

  package_paths.each.each do |filepath|
    begin
      nuget_exec("i #{filepath} -o ./src/packages -source http://www.nuget.org/api/v2/")
    rescue
      puts "Failed to install missing packages ..."      
    end
  end
end


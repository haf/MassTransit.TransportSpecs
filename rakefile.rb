# Copyright Henrik Feldt 2012

require 'albacore'
require 'fileutils'
require 'hpricot'
require 'colorize'

def get_submodules
  modules = Dir.glob("./**/Modules/*/rakefile.rb").collect do |r|
    d = File.dirname r
    return nil if d.nil?
    OpenStruct.new({ 
      :name => (File.basename(d)),
      :dir => d,
      :rakefile => r
    })
  end
  modules.sort_by { |m| m.name == "MassTransit" ? "!MassTransit" : m.name }
end

def with_submodules &blk
  get_submodules.each do |p|
    Dir.chdir p.dir do
      blk.call(p)
    end
  end
end

desc "initialize all submodules"
task :init do
  sh 'git.exe submodule init'
end

desc "build all projects"
task :build => :init do
  with_submodules do |mod|
    sh 'rake --trace' do |ok, res|
      puts "failed with #{res.message}" unless ok
    end
  end
end

desc "submodules.each do ; g pull --ff-only ; end"
task :merge_all do
  with_submodules do |mod|
    sh 'git.exe pull --ff-only'
  end
end

desc "submodules.each do ; g reset --hard HEAD ; end"
task :reset_all do
  with_submodules do |_|
    sh 'git.exe reset --hard HEAD'
    sh 'git.exe clean -fxd'
  end
  sh 'git.exe clean -fxd'
end

desc "run all tests for this project"
task :test do
  puts "TODO! Run tests here."
end

def create_proj_struct path, path_override = nil
  puts "creating proj struct for '#{path}'"
  OpenStruct.new({
    :name => File.basename(path).gsub(/\.(cs|fs)proj/, ''),
    :projfile => path_override || path,
    :xml => (open(path) { |f| Hpricot::XML(f) })
  })
end

def calculate_path proj_path, dep_name, subfolder
  split = proj_path.split("/")
  prefix = "../" * (split.length - 2)
  return prefix, "../MassTransit/#{File.join('src', subfolder, dep_name)}/#{dep_name}.csproj"
end

desc "rewrite all project references"
task :rewrite_refs do
  replace = 
  begin
    with_submodules { |mod|
      if mod.name != "MassTransit" then
        puts "# working in submodule #{mod.name}".colorize( :cyan )
        
        h = OpenStruct.new(mod.marshal_dump.merge({
          :projs => (Dir.glob("./**/*.{csproj,fsproj}").collect{|f| create_proj_struct f})
        }))

        h.projs.each { |p|
          puts "## working in project #{p.name}".colorize( :red )
          refs = []
          [['MassTransit.Log4NetIntegration', 'Loggers'],
           ['MassTransit.TestFramework', ''],
           ['MassTransit.NLogIntegration', 'Loggers'],
           ['MassTransit', '']].
            each{|repl|
              puts "### finding ref to #{repl[0]}"
              p.xml.
                search("/Project/ItemGroup/Reference").
                keep_if{|el| el[:Include].include? repl[0]}.
                remove.
                map{|ref|
                  puts "found ref to #{ref} in #{p.name}"
                  prefix, tpath = calculate_path p.projfile, repl[0], repl[1]
                  puts "tpath: #{tpath}"
                  ref_proj = OpenStruct.new({ 
                    :ref_el => ref, # referencing element
                    :target_proj => (create_proj_struct tpath, (File.join(prefix, tpath)))
                  })
                  refs << ref_proj
                  ref_proj
                }.
                each{|ref_proj|
                  new_ref = %Q[
  <ProjectReference Include="#{ref_proj.target_proj.projfile}">
    <Project>#{(ref_proj.target_proj.xml/"/Project/PropertyGroup/ProjectGuid").innerHTML}</Project>
    <Name>#{ref_proj.target_proj.name}</Name>
  </ProjectReference>]
                  first_ref = (p.xml/"/Project/ItemGroup/ProjectReference")
                  if first_ref.length != 0 then 
                    first_ref.first.before(new_ref)
                  else
                    (p.xml/"/Project/Import").first.before("<ItemGroup>" + new_ref + "</ItemGroup>")
                  end
                  puts "#{ref_proj.ref_el[:Include]} => #{ref_proj.target_proj.projfile}".colorize( :green )
                }
              }
            puts ""
            #puts "Truncating #{p.projfile}, writing:\n #{p.xml}"
            
            File.open(p.projfile, 'w+') { |f|
              f.write (p.xml.to_s)
            }
          }
      end
    }
  rescue Exception => e
    puts "COULD NOT REWRITE REFS!".colorize( :red )
    puts e.to_s
    # clean up changed XXproj files
    with_submodules { |d, rf|
      sh 'git.exe reset --hard HEAD'
    }
    raise e
  end
end

task :default => [:init, :reset_all, :merge_all, :rewrite_refs, :build, :test]
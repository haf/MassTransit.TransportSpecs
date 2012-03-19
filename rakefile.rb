# Copyright Henrik Feldt 2012

require 'albacore'
require 'fileutils'
require 'hpricot'
require 'colorize'

def with_submodules &blk
  Dir.glob("./**/Modules/*/rakefile.rb").each do |r|
    d = File.dirname r
    return if d.nil?
    Dir.chdir d do
      blk.call(OpenStruct.new({ 
        :name => (File.basename(d)),
        :dir => d,
        :rakefile => r
      }))
    end
  end
end

desc "initialize submodules and build all projects"
task :init do
  sh 'git submodule init'
  with_submodules do |mod|
    sh 'rake' do |ok, res|
      puts "failed with #{res.message}" unless ok
    end
  end
end

desc "submodules.each do ; g pull --ff-only ; end"
task :merge_all do
  with_submodules do |mod|
    sh 'git pull --ff-only'
  end
end

desc "reset all submodules' state to HEAD"
task :reset_all do
  with_submodules do |_|
    sh 'git reset --hard HEAD'
    sh 'git clean -fxd'
  end
  sh 'git clean -fxd'
end

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
                  split = p.projfile.split("/")
                  puts split.inspect
                  prefix = "../" * (split.length - 2)
                  tpath = "../MassTransit/#{File.join('src', repl[1], repl[0])}/#{repl[0]}.csproj"
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
    # clean up changed csproj files
    with_submodules { |d, rf|
      sh 'git reset --hard HEAD'
    }
    raise e
  end
end

task :default => [:reset_all, :merge_all, :rewrite_refs, :init, :test]
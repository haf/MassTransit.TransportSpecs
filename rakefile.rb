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

task :test do
  puts "TODO! Run tests here."
end

def create_proj_struct path
  #puts "creating proj struct for '#{path}'"
  OpenStruct.new({
    :name => File.basename(path).gsub(/\.(cs|fs)proj/, ''),
    :projfile => path,
    :xml => (open(path) { |f| Hpricot(f) })
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
          %w[MassTransit MassTransit.Log4NetIntegration MassTransit.TestFramework MassTransit.NLogIntegration].
            each{|repl|
              puts "### finding ref to #{repl}"
              p.xml.
                search("/Project/ItemGroup/Reference").
                keep_if{|el| el[:include].include? repl}.
                remove.
                map{|ref|
                  puts "found ref to #{ref} in #{p.name}"
                  tpath =  "../MassTransit/src/#{repl}/#{repl}.csproj"
                  ref_proj = OpenStruct.new({ 
                    :ref_el => ref, # referencing element
                    :target_proj => (create_proj_struct tpath)
                  })
                  refs << ref_proj
                  ref_proj
                }.
                each{|ref_proj|
                  new_ref = %Q[
  <ProjectReference Include="#{ref_proj.target_proj.projfile}">
    <Project>#{(ref_proj.target_proj.xml/"Project/PropertyGroup/ProjectGuid").innerHTML}</Project>
    <Name>#{ref_proj.target_proj.name}</Name>
  </ProjectReference>]
                  first_ref = (p.xml/"Project/ItemGroup/ProjectReference")
                  if first_ref.length != 0 then 
                    first_ref.first.before new_ref
                  else 
                    (p.xml/"/Project/Import").first.before new_ref
                  end
                  puts "#{ref_proj.ref_el[:include]} => #{ref_proj.target_proj.projfile}".colorize( :green )
                }
              }
            puts ""
            #puts "Truncating #{p.projfile}, writing:\n #{p.xml}"
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

task :default => [:init, :test]
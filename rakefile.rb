# Copyright Henrik Feldt 2012

require 'albacore'
require 'fileutils'

def with_submodules &blk
  Dir.glob("./**/Modules/*/rakefile.rb").each do |r|
    d = File.dirname r
    Dir.chdir d do
      blk.call d, r
    end
  end
end

desc "initialize submodules and build all projects"
task :init do
  sh 'git submodule init'
  with_submodules do |dir, rf|
    sh 'rake' do |ok, res|
      puts "failed with #{res.message}" unless ok
    end
  end
end

desc "submodules.each do ; g pull --ff-only ; end"
task :merge_all do
  with_submodules do |dir, rf|
    sh 'git pull --ff-only'
  end
end

task :test do
  puts "TODO! Run tests here."
end

task :default => [:init, :test]
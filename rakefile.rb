# Copyright Henrik Feldt 2012

require 'albacore'
require 'fileutils'

desc "initialize submodules and build all projects"
task :init do
  sh 'git submodule init'
  Dir.glob("./**/Modules/*/rakefile.rb").each do |r|
    d = File.dirname r
    Dir.chdir d do
      sh 'rake' do |ok, res|
        puts "failed with #{res.message}" unless ok
      end
    end
  end
end

task :test do
  puts "TODO! Run tests here."
end

task :default => [:init, :test]
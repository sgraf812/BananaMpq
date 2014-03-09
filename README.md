BananaMpq
=========

Extracting WoW's geometry data reliably

---------

I use this for NavMesh generation in my private projects.
The solution contains three projects:
  - **BananaMpq**: Contains necessary Mpq extraction logic and can convert it into a Scene made up of WoW's geometry
  - **BananaMpq.Dumper**: Contains a 100-ish example of how to use BananaMpq
  - **BananaMpq.View**: Contains a much more elaborate example rendering extracted ADT geometry. I use this as a prototyping tool.

Since I don't really play WoW any more and consider my time valuable, I did only a rudimentary seperation of my private NavMesh logic from this solution.
I apologize for this mess (see BananaMpq.View/Infrastructure/PluginLoader.cs), but it was infeasible for me to continue maintining 2 versions of BananaMpq side by side.

Regarding MPQ file location:
If you have WoW installed, you can use the code as is. As long as the proper registry keys are available (they are, if you started wow(-64).exe as admin at least once), it should just work.
Otherwise you might want to flip the outcommenting in BananaMpq.Dumper/Program.cs:L20 and in BananaMpq.View/Infrastructure/SceneService.cs:L33

Feel free to use this for whatever projects, it's MIT licensed. 

GL, HF!
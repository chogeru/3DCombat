Scripts and Shaders parameters:

JP_Anime_FixedFPS - the simple script for lock FPS ( for anime will be best from 12 to 24)
Fader - this script for UI image control - 0.0f - Image will be black and solid, 1.0f - UI Image will be Invisible

Shaders:
JP_Base_PBR - basic PBR shader with global grunge map  and "Ghibly" stylize function


JP_FacesToBillboards - ASE sahder function for make your geometry of foliage to billboards ( in to vertex offset input)

JP_GibliShading - this function add "anime" shading effect in your shader


JP_Vegetation_Leafs - this vegetation shader for standart leafs pipeline + ghibly stylize include + wind parameters +global tint mask and brightnes randomizer
JP_Vegetation_Leafs_Particles - this vegetation smart-shader can convert faces to billboards on trees + ghibly stylize include + wind parameters +global tint mask and brightnes randomizer

JP_Vegetation_Trunk - simple shader for bark, will be multiplied on vertex color


JP_Vegetation_Trunk_HQ - pbr shader with moss function for HQ oak and for stones. moss direction can be changed


In Standart render pipline the leafs shaders is have very fast sub-surface scattering functions
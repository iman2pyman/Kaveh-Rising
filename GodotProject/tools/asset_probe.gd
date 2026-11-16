extends SceneTree

func _initialize() -> void:
	call_deferred("_probe")

func _probe() -> void:
	var character_scene := load("res://assets/characters/Barbarian.fbx") as PackedScene
	if character_scene == null:
		push_error("CHARACTER_SCENE_IMPORT_FAILED")
	else:
		var character := character_scene.instantiate()
		print("CHARACTER_ROOT=", character.name)
		_dump_tree(character, 0)
		character.free()
	for file in [
		"Rig_Medium_CombatMelee.glb",
		"Rig_Medium_General.glb",
		"Rig_Medium_MovementAdvanced.glb",
		"Rig_Medium_MovementBasic.glb",
		"Rig_Medium_Special.glb"
	]:
		var scene := load("res://assets/animations/" + file) as PackedScene
		if scene == null:
			push_error("ANIMATION_SCENE_IMPORT_FAILED: " + file)
			continue
		var root := scene.instantiate()
		print("ANIMATION_ASSET=", file)
		_dump_tree(root, 0)
		var player := _find_animation_player(root)
		if player:
			print("ANIMATION_ROOT=", player.root_node)\n\t\t\tprint("ANIMATION_NAMES=", player.get_animation_list())\n\t\t\tif player.get_animation_list().size() > 0:\n\t\t\t\tvar sample := player.get_animation(player.get_animation_list()[0])\n\t\t\t\tif sample.get_track_count() > 0:\n\t\t\t\t\tprint("SAMPLE_TRACK=", sample.track_get_path(0))
		else:
			push_error("ANIMATION_PLAYER_MISSING: " + file)
		root.free()
	quit()

func _dump_tree(node: Node, depth: int) -> void:
	print("  ".repeat(depth), node.name, " <", node.get_class(), ">")
	for child in node.get_children():
		_dump_tree(child, depth + 1)

func _find_animation_player(node: Node) -> AnimationPlayer:
	if node is AnimationPlayer:
		return node as AnimationPlayer
	for child in node.get_children():
		var found := _find_animation_player(child)
		if found:
			return found
	return null

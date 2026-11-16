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
	for file in ["Rig_Medium_CombatMelee.glb", "Rig_Medium_General.glb", "Rig_Medium_MovementAdvanced.glb", "Rig_Medium_MovementBasic.glb", "Rig_Medium_Special.glb"]:
		var scene := load("res://assets/animations/" + file) as PackedScene
		if scene == null:
			push_error("ANIMATION_SCENE_IMPORT_FAILED: " + file)
			continue
		var root := scene.instantiate()
		print("ANIMATION_ASSET=", file)
		var player := _find_animation_player(root)
		if player:
			print("ANIMATION_ROOT=", player.root_node)
			print("ANIMATION_NAMES=", player.get_animation_list())
			if player.get_animation_list().size() > 0:
				var sample := player.get_animation(player.get_animation_list()[0])
				if sample.get_track_count() > 0:
					print("SAMPLE_TRACK=", sample.track_get_path(0))
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

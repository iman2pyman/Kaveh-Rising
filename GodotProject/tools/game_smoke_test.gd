extends SceneTree

func _initialize() -> void:
	call_deferred("_run_test")

func _run_test() -> void:
	var packed := load("res://Main.tscn") as PackedScene
	if packed == null:
		push_error("SMOKE_FAIL: Main.tscn did not load")
		quit(2)
		return
	var game := packed.instantiate()
	root.add_child(game)
	await process_frame
	game.call("_start_new")
	await process_frame
	var hero := game.get("player") as CharacterBody3D
	if hero == null or not hero.has_meta("anim_player"):
		push_error("SMOKE_FAIL: rigged hero was not created")
		quit(3)
		return
	var hero_anim := hero.get_meta("anim_player") as AnimationPlayer
	if hero_anim == null or not hero_anim.has_animation("General/Idle_A"):
		push_error("SMOKE_FAIL: imported idle animation is unavailable")
		quit(4)
		return
	if not hero_anim.has_animation("Combat/Melee_1H_Attack_Chop"):
		push_error("SMOKE_FAIL: combat animation was not merged")
		quit(5)
		return
	var enemies: Array = game.get("enemies")
	if enemies.is_empty():
		push_error("SMOKE_FAIL: first chapter did not spawn enemies")
		quit(6)
		return
	var first_enemy := enemies[0] as CharacterBody3D
	if first_enemy == null or not first_enemy.has_meta("anim_player"):
		push_error("SMOKE_FAIL: rigged enemy was not created")
		quit(7)
		return
	var sounds: Dictionary = game.get("sfx_streams")
	if not sounds.has("swing") or sounds["swing"] == null:
		push_error("SMOKE_FAIL: weapon sound effect did not import")
		quit(8)
		return
	hero_anim.play("Movement/Running_A")
	for i in range(20):
		await process_frame
	print("SMOKE_OK: campaign start, rigged hero/enemy, imported animations and SFX verified")
	game.queue_free()
	quit(0)

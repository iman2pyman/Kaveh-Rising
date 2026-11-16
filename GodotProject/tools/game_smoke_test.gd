extends SceneTree

const Chapters = preload("res://scripts/chapter_data.gd")

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
	game.call("_attack")
	await process_frame
	if hero_anim.current_animation != "Combat/Melee_1H_Attack_Chop":
		push_error("SMOKE_FAIL: player attack animation did not play")
		quit(21)
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
	if Chapters.total_waves() != 17 or Chapters.chapter_count() != 10:
		push_error("SMOKE_FAIL: campaign data is not ten chapters and seventeen waves")
		quit(19)
		return
	game.call("_begin_chapter",1)
	game.call("_clear_enemies")
	game.call("_wave_cleared")
	var wave_after_first_clear := int(game.get("wave"))
	game.call("_wave_cleared")
	if wave_after_first_clear != 2 or int(game.get("wave")) != 2:
		push_error("SMOKE_FAIL: repeated empty frames advanced the wave more than once")
		quit(22)
		return
	game.call("_begin_chapter",0)
	for chapter_index in range(Chapters.chapter_count()):
		game.call("_begin_chapter",chapter_index)
		await process_frame
		var chapter_enemies: Array = game.get("enemies")
		if chapter_enemies.is_empty():
			push_error("SMOKE_FAIL: chapter %d did not spawn combat encounters" % (chapter_index+1))
			quit(20)
			return
	game.call("_begin_chapter",3)
	await process_frame
	var targets: Array = game.get("rescue_targets")
	if targets.size() != 3:
		push_error("SMOKE_FAIL: prison chapter did not create three rescue targets")
		quit(9)
		return
	var rescued_before := int(game.get("rescued"))
	game.call("_interact")
	if int(game.get("rescued")) != rescued_before:
		push_error("SMOKE_FAIL: rescue interaction worked from outside the cage area")
		quit(10)
		return
	var person := targets[0].get("person") as Node3D
	hero.global_position = person.global_position + Vector3(0,0,1)
	game.call("_interact")
	if int(game.get("rescued_this_chapter")) != 1:
		push_error("SMOKE_FAIL: nearby prisoner could not be rescued")
		quit(11)
		return
	game.call("_begin_chapter",9)
	await process_frame
	var boss_list: Array = game.get("enemies")
	if boss_list.size() != 1:
		push_error("SMOKE_FAIL: final boss did not spawn")
		quit(12)
		return
	var boss := boss_list[0] as CharacterBody3D
	var boss_visual := boss.get_node_or_null("Visual")
	if boss_visual == null or boss_visual.get_node_or_null("SerpentHead_1") == null:
		push_error("SMOKE_FAIL: Zahhak's serpent heads were not created")
		quit(13)
		return
	game.set("coins",100)
	var damage_before := float(game.get("attack_damage"))
	game.call("_open_upgrade_menu")
	if not paused:
		push_error("SMOKE_FAIL: upgrade workshop did not pause gameplay")
		quit(16)
		return
	var upgrade_panel := game.get("upgrade_panel") as PanelContainer
	var hammer_upgrade := upgrade_panel.get_child(0).get_child(2) as Button
	hammer_upgrade.emit_signal("pressed")
	if float(game.get("attack_damage")) != damage_before+8.0:
		push_error("SMOKE_FAIL: hammer upgrade did not increase player damage")
		quit(17)
		return
	game.call("_close_upgrade_menu")
	if paused:
		push_error("SMOKE_FAIL: closing upgrade workshop did not resume gameplay")
		quit(18)
		return
	hero_anim.play("Movement/Running_A")
	game.call("_toggle_pause")
	if not paused:
		push_error("SMOKE_FAIL: pause menu did not pause the game")
		quit(14)
		return
	game.call("_toggle_pause")
	if paused:
		push_error("SMOKE_FAIL: pause menu did not resume the game")
		quit(15)
		return
	for i in range(20):
		await process_frame
	print("SMOKE_OK: all 10 chapters / 17 waves, guarded wave transitions, rigged characters, animation sets, SFX, prisoner rescue, Zahhak boss, upgrades and pause menu verified")
	game.queue_free()
	quit(0)

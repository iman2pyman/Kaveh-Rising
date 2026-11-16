extends Node3D

const Chapters = preload("res://scripts/chapter_data.gd")
const MAX_HP := 120.0
const SPEED := 5.5
const DAMAGE := 34.0

var player: CharacterBody3D
var camera: Camera3D
var arena: Node3D
var enemies: Array[CharacterBody3D] = []
var rescue_targets: Array[Dictionary] = []
var max_health := MAX_HP
var attack_damage := DAMAGE
var health := MAX_HP
var stamina := 100.0
var coins := 0
var defeated := 0
var rescued := 0
var rescued_this_chapter := 0
var chapter := 0
var wave := 1
var saved_chapter := 0
var sfx_streams: Dictionary = {}
var sfx_pool: Array[AudioStreamPlayer] = []
var sfx_cursor := 0
var sfx_volume := 0.8
var pause_button: Button
var pause_panel: PanelContainer
var upgrade_button: Button
var upgrade_panel: PanelContainer
var animation_library_cache: Dictionary = {}
var footstep_clock := 0.0
var active := false
var ended := false
var attack_cd := 0.0
var dodge_cd := 0.0
var invulnerable := 0.0
var message_cd := 0.0
var move := Vector2.ZERO
var held := {}
var hero_visual: Node3D
var hero_parts := {}
var hammer: Node3D
var hud: CanvasLayer
var title_panel: PanelContainer
var chapter_text: Label
var objective: Label
var story: Label
var message: Label
var hp_bar: ProgressBar
var stamina_bar: ProgressBar
var enemy_count: Label
var coin_text: Label
var target_panel: PanelContainer
var target_title: Label
var target_bar: ProgressBar
var interact_button: Button

func _ready() -> void:
	randomize()
	_load_save()
	_setup_audio()
	_make_environment()
	_make_player()
	_make_camera()
	_make_hud()
	_show_title()

func _process(delta: float) -> void:
	attack_cd = maxf(0.0, attack_cd - delta)
	dodge_cd = maxf(0.0, dodge_cd - delta)
	invulnerable = maxf(0.0, invulnerable - delta)
	message_cd = maxf(0.0, message_cd - delta)
	if active and not ended and message_cd <= 0.0:
		message.text = objective.text
	_animate_hero(delta)
	_update_target_bar()

func _physics_process(delta: float) -> void:
	if not active or ended:
		return
	var dir: Vector3 = Vector3(move.x, 0.0, move.y)
	if dir.length() > 1.0:
		dir = dir.normalized()
	player.velocity.x = dir.x * SPEED
	player.velocity.z = dir.z * SPEED
	if dir.length() > 0.08:
		player.rotation.y = atan2(-dir.x, -dir.z)
	player.velocity.y -= 22.0 * delta
	player.move_and_slide()
	player.position.x = clampf(player.position.x, -15.2, 15.2)
	player.position.z = clampf(player.position.z, -15.2, 15.2)
	_update_enemies(delta)
	if Vector2(player.velocity.x, player.velocity.z).length() > 1.0:
		footstep_clock -= delta
		if footstep_clock <= 0.0:
			footstep_clock = 0.48
			_play_sfx("grass" if chapter in [1,2,5,6] else "stone")
	else:
		footstep_clock = 0.0
	camera.global_position = camera.global_position.lerp(player.global_position + Vector3(0, 5.4, 8.5), delta * 5.0)
	camera.look_at(player.global_position + Vector3(0, 0.8, -1.0))
	stamina = minf(100.0, stamina + delta * 7.0)
	stamina_bar.value = stamina
	if health <= 0.0:
		_finish(false)

func _setup_audio() -> void:
	sfx_streams = {
		"swing": load("res://assets/audio/weapon-swing.ogg"),
		"impact": load("res://assets/audio/impact-light.ogg"),
		"metal": load("res://assets/audio/metal-clang.ogg"),
		"armor": load("res://assets/audio/armor-hit.ogg"),
		"forge": load("res://assets/audio/forge-chop.ogg"),
		"hammer": load("res://assets/audio/hammer-heavy.ogg"),
		"victory": load("res://assets/audio/victory-bell.ogg"),
		"door": load("res://assets/audio/dungeon-door.ogg"),
		"grass": load("res://assets/audio/footstep-grass.ogg"),
		"stone": load("res://assets/audio/footstep-stone.ogg")
	}
	for i in range(4):
		var player_sfx := AudioStreamPlayer.new()
		player_sfx.volume_db = linear_to_db(maxf(0.001,sfx_volume))
		add_child(player_sfx)
		sfx_pool.append(player_sfx)

func _play_sfx(key: String) -> void:
	if not sfx_streams.has(key) or sfx_streams[key] == null or sfx_pool.is_empty():
		return
	var player_sfx := sfx_pool[sfx_cursor]
	sfx_cursor = (sfx_cursor + 1) % sfx_pool.size()
	player_sfx.stream = sfx_streams[key]
	player_sfx.play()

func _make_environment() -> void:
	var we := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.sky = Sky.new()
	env.sky.sky_material = ProceduralSkyMaterial.new()
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.48, 0.51, 0.60)
	env.ambient_light_energy = 0.8
	we.environment = env
	add_child(we)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -32, 0)
	sun.light_energy = 1.15
	sun.shadow_enabled = true
	add_child(sun)
	arena = Node3D.new()
	arena.name = "Arena"
	add_child(arena)
	_build_arena("forge")

func _mat(color: Color, glow: Color = Color.BLACK) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = 0.82
	if glow != Color.BLACK:
		m.emission_enabled = true
		m.emission = glow
		m.emission_energy_multiplier = 1.5
	return m

func _box(parent: Node, pos: Vector3, size: Vector3, color: Color) -> MeshInstance3D:
	var item := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = size
	item.mesh = mesh
	item.position = pos
	item.material_override = _mat(color)
	parent.add_child(item)
	return item

func _build_arena(kind: String) -> void:
	for child in arena.get_children():
		child.queue_free()
	var floor_color := Color(0.20, 0.19, 0.17)
	match kind:
		"village": floor_color = Color(0.32, 0.25, 0.17)
		"mountain": floor_color = Color(0.29, 0.30, 0.32)
		"dungeon": floor_color = Color(0.12, 0.13, 0.17)
		"plain": floor_color = Color(0.31, 0.27, 0.16)
		"forest": floor_color = Color(0.10, 0.20, 0.13)
		"fortress": floor_color = Color(0.22, 0.19, 0.20)
		"throne": floor_color = Color(0.16, 0.08, 0.12)
	var floor_body := StaticBody3D.new()
	var floor := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(34, 34)
	floor.mesh = plane
	floor.material_override = _mat(floor_color)
	floor_body.add_child(floor)
	var floor_col := CollisionShape3D.new()
	var floor_shape := BoxShape3D.new()
	floor_shape.size = Vector3(34, 0.25, 34)
	floor_col.shape = floor_shape
	floor_col.position.y = -0.13
	floor_body.add_child(floor_col)
	arena.add_child(floor_body)
	for edge in [
		[Vector3(0,1.0,-17),Vector3(34,2,0.6)],
		[Vector3(0,1.0,17),Vector3(34,2,0.6)],
		[Vector3(-17,1.0,0),Vector3(0.6,2,34)],
		[Vector3(17,1.0,0),Vector3(0.6,2,34)]
	]:
		var wall := StaticBody3D.new()
		_box(wall, Vector3.ZERO, edge[1], floor_color.darkened(0.2))
		var col := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = edge[1]
		col.shape = shape
		wall.add_child(col)
		wall.position = edge[0]
		arena.add_child(wall)
	for i in range(20):
		var p := Vector3(randf_range(-14,14), 0, randf_range(-14,14))
		if absf(p.x) < 3.2 and absf(p.z) < 4.0:
			continue
		if kind == "forest":
			var trunk := MeshInstance3D.new()
			var cm := CylinderMesh.new()
			cm.height = 2.8
			cm.top_radius = 0.22
			cm.bottom_radius = 0.36
			trunk.mesh = cm
			trunk.position = p + Vector3(0,1.4,0)
			trunk.material_override = _mat(Color(0.24,0.14,0.08))
			arena.add_child(trunk)
			var crown := MeshInstance3D.new()
			var sm := SphereMesh.new()
			sm.radius = 1.2
			sm.height = 2.2
			crown.mesh = sm
			crown.position = p + Vector3(0,3,0)
			crown.material_override = _mat(Color(0.08,0.25,0.12))
			arena.add_child(crown)
		elif kind in ["mountain","plain"]:
			var rock := MeshInstance3D.new()
			var sm := SphereMesh.new()
			sm.radius = randf_range(0.55,1.1)
			sm.height = sm.radius * 1.5
			rock.mesh = sm
			rock.position = p + Vector3(0,0.45,0)
			rock.scale.y = 1.2
			rock.material_override = _mat(Color(0.32,0.32,0.34))
			arena.add_child(rock)
		elif kind in ["dungeon","fortress","throne"]:
			var pillar := MeshInstance3D.new()
			var cm := CylinderMesh.new()
			cm.height = 3.5
			cm.top_radius = 0.4
			cm.bottom_radius = 0.55
			pillar.mesh = cm
			pillar.position = p + Vector3(0,1.75,0)
			pillar.material_override = _mat(Color(0.25,0.21,0.22))
			arena.add_child(pillar)
		else:
			_box(arena, p + Vector3(0,0.45,0), Vector3(1,0.9,1), Color(0.28,0.17,0.09))
	if kind == "forge":
		for x in [-7.0,7.0]:
			_box(arena, Vector3(x,0.45,-7), Vector3(2.5,0.9,1.5), Color(0.18,0.15,0.13))
			var fire := MeshInstance3D.new()
			var sm := SphereMesh.new()
			sm.radius = 0.48
			sm.height = 0.95
			fire.mesh = sm
			fire.position = Vector3(x,1.1,-7)
			fire.material_override = _mat(Color(1,0.28,0.03), Color(1,0.12,0.01))
			arena.add_child(fire)
	if kind == "throne":
		_box(arena, Vector3(0,0.35,-10), Vector3(7,0.7,4), Color(0.20,0.07,0.10))

func _make_player() -> void:
	player = CharacterBody3D.new()
	player.name = "Kaveh"
	player.position = Vector3(0,0,5)
	_make_humanoid(player, true, Color(0.56,0.16,0.10), Color(0.17,0.20,0.25), 1.0)
	var col := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.42
	shape.height = 1.9
	col.shape = shape
	col.position.y = 0.95
	player.add_child(col)
	add_child(player)

func _make_humanoid(body: CharacterBody3D, hero: bool, tunic: Color, armor: Color, size: float) -> void:
	if _attach_rigged_model(body, hero, size):
		return
	var visual: Node3D = Node3D.new()
	visual.name = "Visual"
	visual.scale = Vector3.ONE * size
	body.add_child(visual)
	var torso := MeshInstance3D.new()
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.43
	capsule.height = 1.05
	torso.mesh = capsule
	torso.position.y = 1.05
	torso.material_override = _mat(tunic)
	visual.add_child(torso)
	var chest := MeshInstance3D.new()
	var chest_mesh := BoxMesh.new()
	chest_mesh.size = Vector3(0.68,0.28,0.16)
	chest.mesh = chest_mesh
	chest.position = Vector3(0,1.25,-0.34)
	chest.material_override = _mat(armor)
	visual.add_child(chest)
	var head := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.29
	sphere.height = 0.58
	head.mesh = sphere
	head.position.y = 1.82
	head.material_override = _mat(Color(0.69,0.48,0.34))
	visual.add_child(head)
	var helmet := MeshInstance3D.new()
	var helm_mesh := SphereMesh.new()
	helm_mesh.radius = 0.33
	helm_mesh.height = 0.35
	helmet.mesh = helm_mesh
	helmet.position.y = 2.02
	helmet.material_override = _mat(Color(0.48,0.35,0.16) if hero else Color(0.12,0.15,0.20))
	visual.add_child(helmet)
	var beard := MeshInstance3D.new()
	var beard_mesh := BoxMesh.new()
	beard_mesh.size = Vector3(0.27,0.34,0.16)
	beard.mesh = beard_mesh
	beard.position = Vector3(0,1.56,-0.24)
	beard.material_override = _mat(Color(0.12,0.07,0.04))
	visual.add_child(beard)
	var parts := {}
	for side in [-1,1]:
		var arm_pivot := Node3D.new()
		arm_pivot.position = Vector3(float(side)*0.53,1.42,0)
		visual.add_child(arm_pivot)
		var arm := MeshInstance3D.new()
		var arm_mesh := CapsuleMesh.new()
		arm_mesh.radius = 0.16
		arm_mesh.height = 0.82
		arm.mesh = arm_mesh
		arm.position.y = -0.34
		arm.material_override = _mat(tunic)
		arm_pivot.add_child(arm)
		var leg_pivot := Node3D.new()
		leg_pivot.position = Vector3(float(side)*0.23,0.72,0)
		visual.add_child(leg_pivot)
		var leg := MeshInstance3D.new()
		var leg_mesh := CapsuleMesh.new()
		leg_mesh.radius = 0.19
		leg_mesh.height = 0.78
		leg.mesh = leg_mesh
		leg.position.y = -0.34
		leg.material_override = _mat(armor)
		leg_pivot.add_child(leg)
		parts["arm"+str(side)] = arm_pivot
		parts["leg"+str(side)] = leg_pivot
	if hero:
		hero_visual = visual
		hero_parts = parts
		hammer = Node3D.new()
		hammer.position = Vector3(0.53,1.12,-0.25)
		visual.add_child(hammer)
		var handle := MeshInstance3D.new()
		var handle_mesh := CylinderMesh.new()
		handle_mesh.height = 0.95
		handle_mesh.top_radius = 0.055
		handle_mesh.bottom_radius = 0.07
		handle.mesh = handle_mesh
		handle.rotation.z = -0.35
		handle.material_override = _mat(Color(0.25,0.13,0.06))
		hammer.add_child(handle)
		var head_part := MeshInstance3D.new()
		var head_box := BoxMesh.new()
		head_box.size = Vector3(0.58,0.28,0.3)
		head_part.mesh = head_box
		head_part.position.y = 0.45
		head_part.material_override = _mat(Color(0.43,0.45,0.48))
		hammer.add_child(head_part)

func _attach_rigged_model(body: CharacterBody3D, hero: bool, size: float) -> bool:
	var model_path := "res://assets/characters/Barbarian.fbx" if hero else "res://assets/characters/Mage.fbx"
	var packed := load(model_path) as PackedScene
	if packed == null:
		push_warning("Rigged model unavailable, using procedural fallback: " + model_path)
		return false
	var model := packed.instantiate()
	model.name = "Visual"
	model.scale = Vector3.ONE * size
	body.add_child(model)
	var anim_player := AnimationPlayer.new()
	anim_player.name = "CharacterAnimationPlayer"
	anim_player.root_node = NodePath("..")
	model.add_child(anim_player)
	var animation_files := {
		"Combat": "Rig_Medium_CombatMelee.glb",
		"General": "Rig_Medium_General.glb",
		"Advanced": "Rig_Medium_MovementAdvanced.glb",
		"Movement": "Rig_Medium_MovementBasic.glb",
		"Special": "Rig_Medium_Special.glb"
	}
	for library_name in animation_files:
		var library := _get_animation_library(library_name, str(animation_files[library_name]))
		if library != null:
			anim_player.add_animation_library(library_name, library)
	body.set_meta("anim_player", anim_player)
	body.set_meta("rigged", true)
	if hero:
		hero_visual = model
		hero_parts = {}
		hammer = Node3D.new()
		hammer.name = "WarHammer"
		hammer.position = Vector3(0.52, 1.05, -0.30)
		model.add_child(hammer)
		var handle := MeshInstance3D.new()
		var handle_mesh := CylinderMesh.new()
		handle_mesh.height = 0.95
		handle_mesh.top_radius = 0.055
		handle_mesh.bottom_radius = 0.07
		handle.mesh = handle_mesh
		handle.rotation.z = -0.35
		handle.material_override = _mat(Color(0.24, 0.13, 0.06))
		hammer.add_child(handle)
		var hammer_head := MeshInstance3D.new()
		var head_mesh := BoxMesh.new()
		head_mesh.size = Vector3(0.58, 0.28, 0.30)
		hammer_head.mesh = head_mesh
		hammer_head.position.y = 0.45
		hammer_head.material_override = _mat(Color(0.44, 0.46, 0.49))
		hammer.add_child(hammer_head)
		_play_anim(body, "General/Idle_A")
	else:
		_play_anim(body, "General/Idle_A")
	return true

func _get_animation_library(library_name: String, file_name: String) -> AnimationLibrary:
	if animation_library_cache.has(library_name):
		return animation_library_cache[library_name]
	var packed := load("res://assets/animations/" + file_name) as PackedScene
	if packed == null:
		push_warning("Animation library missing: " + file_name)
		return null
	var source_root := packed.instantiate()
	var source_player := _find_animation_player(source_root)
	if source_player == null:
		source_root.free()
		push_warning("AnimationPlayer missing in " + file_name)
		return null
	var library := AnimationLibrary.new()
	for animation_name in source_player.get_animation_list():
		var source_animation := source_player.get_animation(animation_name)
		if source_animation != null:
			library.add_animation(str(animation_name), source_animation.duplicate(true))
	source_root.free()
	animation_library_cache[library_name] = library
	return library

func _find_animation_player(node: Node) -> AnimationPlayer:
	if node is AnimationPlayer:
		return node as AnimationPlayer
	for child in node.get_children():
		var found := _find_animation_player(child)
		if found != null:
			return found
	return null

func _play_anim(body: CharacterBody3D, animation_name: String) -> void:
	if not body.has_meta("anim_player"):
		return
	var anim_player := body.get_meta("anim_player") as AnimationPlayer
	if anim_player == null or not anim_player.has_animation(animation_name):
		return
	if anim_player.current_animation != animation_name:
		anim_player.play(animation_name)

func _make_camera() -> void:
	camera = Camera3D.new()
	camera.position = Vector3(0,5.4,8.5)
	camera.rotation_degrees.x = -25
	camera.current = true
	player.add_child(camera)

func _make_hud() -> void:
	hud = CanvasLayer.new()
	hud.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(hud)
	var top := PanelContainer.new()
	top.position = Vector2(14,12)
	top.custom_minimum_size = Vector2(330,145)
	hud.add_child(top)
	var vbox := VBoxContainer.new()
	top.add_child(vbox)
	chapter_text = Label.new()
	chapter_text.add_theme_font_size_override("font_size",21)
	vbox.add_child(chapter_text)
	hp_bar = _bar(vbox,"سلامتی کاوه",max_health,health)
	stamina_bar = _bar(vbox,"توان رزمی",100,stamina)
	objective = Label.new()
	objective.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	objective.custom_minimum_size = Vector2(300,35)
	vbox.add_child(objective)
	var row := HBoxContainer.new()
	vbox.add_child(row)
	enemy_count = Label.new()
	row.add_child(enemy_count)
	coin_text = Label.new()
	row.add_child(coin_text)
	story = Label.new()
	story.position = Vector2(18,165)
	story.size = Vector2(650,55)
	story.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	story.add_theme_font_size_override("font_size",17)
	hud.add_child(story)
	message = Label.new()
	message.set_anchors_preset(Control.PRESET_CENTER_TOP)
	message.position = Vector2(-260,15)
	message.size = Vector2(520,40)
	message.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	message.add_theme_font_size_override("font_size",22)
	hud.add_child(message)
	target_panel = PanelContainer.new()
	target_panel.set_anchors_preset(Control.PRESET_CENTER_TOP)
	target_panel.position = Vector2(-145,65)
	target_panel.custom_minimum_size = Vector2(290,60)
	hud.add_child(target_panel)
	var target_box := VBoxContainer.new()
	target_panel.add_child(target_box)
	target_title = Label.new()
	target_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	target_box.add_child(target_title)
	target_bar = ProgressBar.new()
	target_box.add_child(target_bar)
	target_panel.hide()
	_make_controls()
	title_panel = PanelContainer.new()
	title_panel.set_anchors_preset(Control.PRESET_CENTER)
	title_panel.position = Vector2(-230,-170)
	title_panel.custom_minimum_size = Vector2(460,340)
	hud.add_child(title_panel)

func _bar(parent: VBoxContainer, caption: String, maximum: float, value: float) -> ProgressBar:
	var group := VBoxContainer.new()
	parent.add_child(group)
	var label := Label.new()
	label.text = caption
	group.add_child(label)
	var bar := ProgressBar.new()
	bar.max_value = maximum
	bar.value = value
	bar.custom_minimum_size = Vector2(300,14)
	group.add_child(bar)
	return bar

func _make_controls() -> void:
	var pad := GridContainer.new()
	pad.columns = 3
	pad.set_anchors_preset(Control.PRESET_BOTTOM_LEFT)
	pad.position = Vector2(18,-168)
	hud.add_child(pad)
	var entries := [
		["↖",Vector2(-1,-1)],["↑",Vector2(0,-1)],["↗",Vector2(1,-1)],
		["←",Vector2(-1,0)],["●",Vector2.ZERO],["→",Vector2(1,0)],
		["↙",Vector2(-1,1)],["↓",Vector2(0,1)],["↘",Vector2(1,1)]
	]
	for entry in entries:
		var b := Button.new()
		b.text = entry[0]
		b.custom_minimum_size = Vector2(62,47)
		b.button_down.connect(_press_dir.bind(entry[1]))
		b.button_up.connect(_release_dir.bind(entry[1]))
		pad.add_child(b)
	var hit := _control_button("ضربه",Vector2(-150,-112),Vector2(130,72))
	hit.pressed.connect(_attack)
	var dodge := _control_button("جاخالی",Vector2(-150,-194),Vector2(130,66))
	dodge.pressed.connect(_dodge)
	interact_button = _control_button("آزادسازی",Vector2(-150,-270),Vector2(130,60))
	interact_button.pressed.connect(_interact)
	interact_button.hide()
	upgrade_button = _control_button("ارتقا",Vector2(-150,-340),Vector2(130,58))
	upgrade_button.pressed.connect(_open_upgrade_menu)
	pause_button = Button.new()
	pause_button.text = "Ⅱ"
	pause_button.tooltip_text = "توقف بازی"
	pause_button.set_anchors_preset(Control.PRESET_TOP_RIGHT)
	pause_button.position = Vector2(-66,14)
	pause_button.size = Vector2(52,48)
	pause_button.add_theme_font_size_override("font_size",24)
	pause_button.pressed.connect(_toggle_pause)
	hud.add_child(pause_button)
	pause_button.hide()

func _control_button(caption: String, pos: Vector2, size: Vector2) -> Button:
	var b := Button.new()
	b.text = caption
	b.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT)
	b.position = pos
	b.size = size
	b.add_theme_font_size_override("font_size",21)
	hud.add_child(b)
	return b

func _show_title() -> void:
	for child in title_panel.get_children():
		child.queue_free()
	var box := VBoxContainer.new()
	title_panel.add_child(box)
	var title := Label.new()
	title.text = "قیام کاوه"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size",38)
	box.add_child(title)
	var sub := Label.new()
	sub.text = "KAVEH RISING  |  حماسه‌ای از اسطوره‌های ایران"
	sub.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	box.add_child(sub)
	var intro := Label.new()
	intro.text = "۱۰ فصل داستانی • ۱۷ موج نبرد\nمبارزه، نجات مردم و نبرد پایانی با ضحاک"
	intro.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	intro.custom_minimum_size = Vector2(420,75)
	box.add_child(intro)
	var start := Button.new()
	start.text = "شروع خیزش"
	start.custom_minimum_size.y = 55
	start.pressed.connect(_start_new)
	box.add_child(start)
	var resume := Button.new()
	resume.text = "ادامه از فصل ذخیره‌شده"
	resume.pressed.connect(_resume)
	box.add_child(resume)
	var hint := Label.new()
	hint.text = "حرکت: جهت‌ها  |  ضربه: حمله  |  جاخالی: فرار"
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	box.add_child(hint)
	title_panel.show()

func _start_new() -> void:
	health = max_health
	coins = 0
	defeated = 0
	rescued = 0
	_begin_chapter(0)

func _resume() -> void:
	health = MAX_HP
	_begin_chapter(saved_chapter)

func _begin_chapter(index: int) -> void:
	chapter = clampi(index,0,Chapters.chapter_count()-1)
	wave = 1
	rescued_this_chapter = 0
	active = true
	ended = false
	title_panel.hide()
	var data: Dictionary = Chapters.get_chapter(chapter)
	_build_arena(str(data.arena))
	if chapter == 3 or chapter == 4:
		_spawn_rescue_targets()
	if chapter == 8:
		_make_kaviani_banner()
	player.position = Vector3(0,0,5)
	player.velocity = Vector3.ZERO
	health = minf(max_health,health+25)
	hp_bar.value = health
	chapter_text.text = "فصل %d از ۱۰  |  %s" % [chapter+1,data.title]
	objective.text = str(data.mission)
	story.text = str(data.story)
	_show_message(str(data.subtitle))
	interact_button.visible = chapter == 3 or chapter == 4
	pause_button.show()
	_spawn_wave()

func _spawn_wave() -> void:
	_clear_enemies()
	var data: Dictionary = Chapters.get_chapter(chapter)
	var count := int(data.count) + (1 if wave > 1 else 0)
	if chapter == 9:
		count = 1
	for i in range(count):
		var kind := str(data.enemy)
		if chapter == 9:
			kind = "boss"
		elif wave > 1 and i == count-1:
			kind = "elite"
		var angle := TAU * float(i) / float(maxi(1,count))
		var radius := randf_range(7.5,10.5)
		_spawn_enemy(kind,Vector3(cos(angle)*radius,0,-radius*0.55+sin(angle)*radius*0.5))
	_update_counts()

func _make_beast(body: CharacterBody3D, size: float) -> void:
	var visual := Node3D.new()
	visual.name = "Visual"
	visual.scale = Vector3.ONE * size
	body.add_child(visual)
	var torso := MeshInstance3D.new()
	var torso_mesh := SphereMesh.new()
	torso_mesh.radius = 0.72
	torso_mesh.height = 1.45
	torso.mesh = torso_mesh
	torso.position = Vector3(0,0.95,0.05)
	torso.material_override = _mat(Color(0.18,0.25,0.12))
	visual.add_child(torso)
	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.48
	head_mesh.height = 0.72
	head.mesh = head_mesh
	head.position = Vector3(0,1.30,-0.72)
	head.material_override = _mat(Color(0.24,0.30,0.15))
	visual.add_child(head)
	for side in [-1,1]:
		var horn := MeshInstance3D.new()
		var horn_mesh := CylinderMesh.new()
		horn_mesh.top_radius = 0.015
		horn_mesh.bottom_radius = 0.13
		horn_mesh.height = 0.72
		horn.mesh = horn_mesh
		horn.position = Vector3(float(side)*0.30,1.83,-0.65)
		horn.rotation.z = float(side)*-0.42
		horn.material_override = _mat(Color(0.42,0.37,0.24))
		visual.add_child(horn)
		var eye := MeshInstance3D.new()
		var eye_mesh := SphereMesh.new()
		eye_mesh.radius = 0.075
		eye.mesh = eye_mesh
		eye.position = Vector3(float(side)*0.28,1.40,-1.12)
		eye.material_override = _mat(Color(1.0,0.12,0.03),Color(1.0,0.04,0.01))
		visual.add_child(eye)
	for side in [-1,1]:
		for fore in [-1,1]:
			var leg := Node3D.new()
			leg.name = "leg_%d_%d" % [side,fore]
			leg.position = Vector3(float(side)*0.43,0.63, float(fore)*0.42)
			visual.add_child(leg)
			var limb := MeshInstance3D.new()
			var limb_mesh := CapsuleMesh.new()
			limb_mesh.radius = 0.18
			limb_mesh.height = 0.72
			limb.mesh = limb_mesh
			limb.position.y = -0.28
			limb.material_override = _mat(Color(0.14,0.19,0.10))
			leg.add_child(limb)

func _decorate_zahhak(body: CharacterBody3D) -> void:
	var visual := body.get_node_or_null("Visual")
	if visual == null:
		return
	for side in [-1,1]:
		var serpent := Node3D.new()
		serpent.name = "SerpentHead_%d" % side
		serpent.position = Vector3(float(side)*0.50,1.75,0.12)
		visual.add_child(serpent)
		for segment in range(3):
			var neck := MeshInstance3D.new()
			var neck_mesh := CapsuleMesh.new()
			neck_mesh.radius = 0.17 - float(segment)*0.025
			neck_mesh.height = 0.48
			neck.mesh = neck_mesh
			neck.position = Vector3(float(side)*float(segment)*0.16,0.20+float(segment)*0.25,0)
			neck.rotation.z = float(side)*-0.35
			neck.material_override = _mat(Color(0.10,0.25,0.12))
			serpent.add_child(neck)
		var snake_head := MeshInstance3D.new()
		var head_mesh := SphereMesh.new()
		head_mesh.radius = 0.25
		head_mesh.height = 0.42
		snake_head.mesh = head_mesh
		snake_head.position = Vector3(float(side)*0.50,0.95,-0.02)
		snake_head.material_override = _mat(Color(0.13,0.31,0.15))
		serpent.add_child(snake_head)
		for eye_side in [-1,1]:
			var eye := MeshInstance3D.new()
			var eye_mesh := SphereMesh.new()
			eye_mesh.radius = 0.055
			eye.mesh = eye_mesh
			eye.position = Vector3(float(eye_side)*0.14,1.00,-0.20)
			eye.material_override = _mat(Color(1,0.08,0.02),Color(1,0.02,0.0))
			serpent.add_child(eye)

func _spawn_rescue_targets() -> void:
	_clear_rescue_targets()
	var packed := load("res://assets/characters/Mage.fbx") as PackedScene
	var positions := [Vector3(-7,0,-5),Vector3(0,0,-9),Vector3(7,0,-5)]
	for i in range(positions.size()):
		var pos: Vector3 = positions[i]
		var cage := Node3D.new()
		cage.position = pos
		arena.add_child(cage)
		for side in [-1,1]:
			for depth in [-1,1]:
				var bar := MeshInstance3D.new()
				var bar_mesh := CylinderMesh.new()
				bar_mesh.top_radius = 0.045
				bar_mesh.bottom_radius = 0.045
				bar_mesh.height = 2.4
				bar.mesh = bar_mesh
				bar.position = Vector3(float(side)*0.85,1.2,float(depth)*0.75)
				bar.material_override = _mat(Color(0.38,0.40,0.43))
				cage.add_child(bar)
		_box(cage,Vector3(0,2.35,0),Vector3(1.8,0.12,1.6),Color(0.28,0.29,0.31))
		var person := Node3D.new()
		person.position = pos + Vector3(0,0,-0.15)
		arena.add_child(person)
		if packed:
			var model := packed.instantiate()
			model.scale = Vector3.ONE*0.52
			person.add_child(model)
		else:
			_box(person,Vector3(0,0.9,0),Vector3(0.55,1.6,0.4),Color(0.55,0.43,0.32))
		rescue_targets.append({"person":person,"cage":cage,"position":pos})

func _clear_rescue_targets() -> void:
	for target in rescue_targets:
		if is_instance_valid(target.get("person")):
			target.person.queue_free()
		if is_instance_valid(target.get("cage")):
			target.cage.queue_free()
	rescue_targets.clear()

func _make_kaviani_banner() -> void:
	var pole := MeshInstance3D.new()
	var pole_mesh := CylinderMesh.new()
	pole_mesh.top_radius = 0.07
	pole_mesh.bottom_radius = 0.11
	pole_mesh.height = 5.8
	pole.mesh = pole_mesh
	pole.position = Vector3(-8,2.9,-7)
	pole.material_override = _mat(Color(0.55,0.40,0.18))
	arena.add_child(pole)
	var cloth := _box(arena,Vector3(-6.9,3.6,-7),Vector3(2.1,1.7,0.12),Color(0.62,0.08,0.09))
	var emblem := MeshInstance3D.new()
	var emblem_mesh := SphereMesh.new()
	emblem_mesh.radius = 0.42
	emblem_mesh.height = 0.2
	emblem.mesh = emblem_mesh
	emblem.position = Vector3(-6.9,3.6,-6.88)
	emblem.material_override = _mat(Color(0.94,0.68,0.22))
	arena.add_child(emblem)

func _spawn_enemy(kind: String, pos: Vector3) -> void:
	var enemy := CharacterBody3D.new()
	enemy.position = pos
	var tunic := Color(0.12,0.15,0.21)
	var armor := Color(0.27,0.29,0.33)
	var hp := 78.0
	var power := 7.0
	var speed := 2.0
	var scale := 0.96
	match kind:
		"elite":
			tunic = Color(0.34,0.07,0.10)
			armor = Color(0.49,0.34,0.15)
			hp = 150
			power = 13
			speed = 2.3
			scale = 1.08
		"beast":
			tunic = Color(0.17,0.25,0.12)
			armor = Color(0.31,0.35,0.20)
			hp = 110
			power = 11
			speed = 3.0
			scale = 1.12
		"boss":
			tunic = Color(0.25,0.02,0.07)
			armor = Color(0.58,0.32,0.08)
			hp = 1050
			power = 22
			speed = 1.7
			scale = 1.72
	if kind == "beast":
		_make_beast(enemy,scale)
	else:
		_make_humanoid(enemy,false,tunic,armor,scale)
		if kind == "boss":
			_decorate_zahhak(enemy)
		_play_anim(enemy,"General/Idle_A")
	var col := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.43*scale
	shape.height = 1.9*scale
	col.shape = shape
	col.position.y = 0.95*scale
	enemy.add_child(col)
	enemy.set_meta("kind",kind)
	enemy.set_meta("hp",hp)
	enemy.set_meta("max_hp",hp)
	enemy.set_meta("power",power)
	enemy.set_meta("speed",speed)
	enemy.set_meta("cooldown",randf_range(0.4,1.3))
	enemy.set_meta("phase",randf()*TAU)
	add_child(enemy)
	enemies.append(enemy)

func _update_enemies(delta: float) -> void:
	for enemy in enemies.duplicate():
		if not is_instance_valid(enemy):
			enemies.erase(enemy)
			continue
		var offset: Vector3 = player.global_position-enemy.global_position
		offset.y = 0
		var distance: float = offset.length()
		var enemy_speed: float = float(enemy.get_meta("speed"))
		var enemy_power: float = float(enemy.get_meta("power"))
		var reach: float = 2.2 if enemy.get_meta("kind") == "boss" else 1.6
		if enemy.get_meta("kind") == "boss":
			var hp_ratio := float(enemy.get_meta("hp")) / float(enemy.get_meta("max_hp"))
			var rage_phase := 0
			if hp_ratio <= 0.25:
				rage_phase = 2
				enemy_speed = 2.75
				enemy_power = 32.0
				reach = 3.0
			elif hp_ratio <= 0.60:
				rage_phase = 1
				enemy_speed = 2.2
				enemy_power = 25.0
				reach = 2.6
			if int(enemy.get_meta("rage_phase",0)) != rage_phase:
				enemy.set_meta("rage_phase",rage_phase)
				if rage_phase > 0:
					_show_message("ضحاک خشمگین‌تر شده است!")
					_play_sfx("metal")
		if distance > reach:
			_play_anim(enemy,"Movement/Running_A")
			var dir: Vector3 = offset.normalized()
			enemy.velocity.x = dir.x*enemy_speed
			enemy.velocity.z = dir.z*enemy_speed
			enemy.rotation.y = atan2(-dir.x,-dir.z)
		else:
			_play_anim(enemy,"General/Idle_A")
			enemy.velocity.x = 0
			enemy.velocity.z = 0
			var cd := float(enemy.get_meta("cooldown"))-delta
			if cd <= 0:
				cd = randf_range(1.0,1.6)
				if invulnerable <= 0:
					_take_damage(enemy_power)
			enemy.set_meta("cooldown",cd)
		enemy.velocity.y -= 22*delta
		enemy.move_and_slide()
		enemy.position.x = clampf(enemy.position.x,-15.2,15.2)
		enemy.position.z = clampf(enemy.position.z,-15.2,15.2)
		var visual: Node = enemy.get_node_or_null("Visual")
		if visual:
			var phase := float(enemy.get_meta("phase"))+delta*7
			enemy.set_meta("phase",phase)
			for part in visual.get_children():
				if part is Node3D and str(part.name).begins_with("arm"):
					part.rotation.x = sin(phase)*0.18
				elif part is Node3D and str(part.name).begins_with("leg"):
					part.rotation.x = sin(phase+PI)*0.14
	if enemies.is_empty() and active and not ended:
		_wave_cleared()

func _attack() -> void:
	if not active or ended or attack_cd > 0:
		return
	attack_cd = 0.42
	_play_anim(player,"Combat/Melee_1H_Attack_Chop")
	_play_sfx("swing")
	hammer.rotation.x = -1.0
	var hit := false
	for enemy in enemies.duplicate():
		if not is_instance_valid(enemy):
			continue
		var offset: Vector3 = enemy.global_position-player.global_position
		var reach := 3.0 if enemy.get_meta("kind") == "boss" else 2.7
		if offset.length() <= reach and (-player.global_transform.basis.z).dot(offset.normalized()) > -0.15:
			var hp := maxf(0,float(enemy.get_meta("hp"))-attack_damage-(8 if chapter == 8 else 0))
			enemy.set_meta("hp",hp)
			hit = true
			_hit_effect(enemy.global_position+Vector3(0,1,0))
			if hp <= 0:
				defeated += 1
				coins += 100 if enemy.get_meta("kind") == "boss" else 10
				_defeat_enemy(enemy)
			else:
				_play_anim(enemy,"General/Hit_A")
	if hit:
		_play_sfx("metal")
		_show_message("ضربه سنگین کاوه!")
	else:
		_show_message("رو به دشمن بایست و نزدیک‌تر شو")
	_update_counts()
	_update_target_bar()

func _dodge() -> void:
	if not active or ended or dodge_cd > 0 or stamina < 25:
		return
	dodge_cd = 1.15
	_play_anim(player,"Advanced/Dodge_Backward")
	_play_sfx("armor")
	stamina -= 25
	var dir: Vector3 = Vector3(move.x,0,move.y)
	if dir.length() < 0.1:
		dir = -player.global_transform.basis.z
	player.velocity.x = dir.normalized().x*12
	player.velocity.z = dir.normalized().z*12
	invulnerable = 0.65
	_show_message("جاخالی به‌موقع!")

func _interact() -> void:
	if not active or (chapter != 3 and chapter != 4):
		return
	var nearest_index := -1
	var nearest_distance: float = 2.8
	for i in range(rescue_targets.size()):
		var person := rescue_targets[i].get("person") as Node3D
		if is_instance_valid(person):
			var distance := player.global_position.distance_to(person.global_position)
			if distance < nearest_distance:
				nearest_distance = distance
				nearest_index = i
	if nearest_index < 0:
		_show_message("برای آزادسازی به یکی از قفس‌ها نزدیک شو")
		return
	var target: Dictionary = rescue_targets[nearest_index]
	var person := target.get("person") as Node3D
	var cage := target.get("cage") as Node3D
	if is_instance_valid(person):
		person.queue_free()
	if is_instance_valid(cage):
		cage.queue_free()
	rescue_targets.remove_at(nearest_index)
	rescued += 1
	rescued_this_chapter += 1
	_play_anim(player,"General/Interact")
	_play_sfx("door")
	health = minf(max_health,health+12)
	hp_bar.value = health
	_show_message("زندانی آزاد شد؛ مردم به خیزش می‌پیوندند (%d از ۳)" % mini(rescued_this_chapter,3))
	if rescue_targets.is_empty():
		interact_button.hide()

func _take_damage(amount: float) -> void:
	if invulnerable > 0:
		return
	health = maxf(0,health-amount)
	_play_anim(player,"General/Hit_A")
	_play_sfx("armor")
	hp_bar.value = health
	invulnerable = 0.45
	_show_message("ضربه خوردی؛ جاخالی بده!")

func _defeat_enemy(enemy: CharacterBody3D) -> void:
	_play_anim(enemy,"General/Death_A")
	enemy.velocity = Vector3.ZERO
	for child in enemy.get_children():
		if child is CollisionShape3D:
			child.set_deferred("disabled",true)
	enemies.erase(enemy)
	get_tree().create_timer(1.15).timeout.connect(func():
		if is_instance_valid(enemy):
			enemy.queue_free()
	)

func _hit_effect(pos: Vector3) -> void:
	var spark := MeshInstance3D.new()
	var mesh := SphereMesh.new()
	mesh.radius = 0.18
	mesh.height = 0.36
	spark.mesh = mesh
	spark.position = pos
	spark.material_override = _mat(Color(1,0.55,0.12),Color(1,0.2,0.02))
	arena.add_child(spark)
	var tween := create_tween()
	tween.tween_property(spark,"scale",Vector3(2.5,2.5,2.5),0.2)
	tween.parallel().tween_property(spark,"transparency",1.0,0.2)
	tween.tween_callback(spark.queue_free)

func _wave_cleared() -> void:
	var data: Dictionary = Chapters.get_chapter(chapter)
	if wave < int(data.waves):
		wave += 1
		objective.text = "%s  |  موج %d از %d" % [data.mission,wave,data.waves]
		_show_message("موج بعدی دشمنان در راه است")
		get_tree().create_timer(1.6).timeout.connect(_spawn_wave)
	else:
		_play_sfx("victory")
		coins += int(data.reward)
		_save(chapter+1)
		if chapter == 9:
			_finish(true)
		else:
			_chapter_complete()

func _chapter_complete() -> void:
	ended = true
	_dialog("فصل %d به پایان رسید" % (chapter+1),"پاداش: %d سکه\nبرای ادامه داستان آماده‌ای؟" % int(Chapters.get_chapter(chapter).reward),"ادامه",func():
		_begin_chapter(chapter+1)
	)

func _finish(won: bool) -> void:
	ended = true
	active = false
	pause_button.hide()
	var title := "آزادی ایران!" if won else "کاوه زخمی شد"
	var body := "ضحاک شکست خورد و درفش کاویانی برافراشته شد.\nدشمنان شکست‌خورده: %d" % defeated if won else "این نبرد پایان راه نیست. دوباره برخیز و قیام را ادامه بده."
	_dialog(title,body,"بازگشت به آغاز",func():
		_show_title()
	)

func _dialog(title_text: String, body_text: String, button_text: String, action: Callable) -> void:
	var panel := PanelContainer.new()
	panel.set_anchors_preset(Control.PRESET_CENTER)
	panel.position = Vector2(-220,-145)
	panel.custom_minimum_size = Vector2(440,290)
	hud.add_child(panel)
	var box := VBoxContainer.new()
	panel.add_child(box)
	var title := Label.new()
	title.text = title_text
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size",28)
	box.add_child(title)
	var body := Label.new()
	body.text = body_text
	body.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	body.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	body.custom_minimum_size = Vector2(410,110)
	box.add_child(body)
	var button := Button.new()
	button.text = button_text
	button.custom_minimum_size.y = 55
	button.pressed.connect(func():
		panel.queue_free()
		action.call()
	)
	box.add_child(button)

func _update_counts() -> void:
	enemy_count.text = "دشمنان: %d   " % enemies.size()
	coin_text.text = "سکه: %d" % coins

func _update_target_bar() -> void:
	var nearest: CharacterBody3D = null
	var distance: float = 999.0
	for enemy in enemies:
		if is_instance_valid(enemy):
			var d := player.global_position.distance_to(enemy.global_position)
			if d < distance:
				distance = d
				nearest = enemy
	if nearest:
		target_panel.show()
		target_title.text = "ضحاک" if nearest.get_meta("kind") == "boss" else ("موجود افسانه‌ای" if nearest.get_meta("kind") == "beast" else "سرباز ضحاک")
		target_bar.max_value = float(nearest.get_meta("max_hp"))
		target_bar.value = float(nearest.get_meta("hp"))
	else:
		target_panel.hide()

func _animate_hero(delta: float) -> void:
	if not is_instance_valid(hero_visual):
		return
	var moving := Vector2(player.velocity.x,player.velocity.z).length() > 0.5
	if attack_cd <= 0.2:
		_play_anim(player,"Movement/Running_A" if moving else "General/Idle_A")
	if not hero_parts.is_empty():
		var phase := float(hero_visual.get_meta("phase",0.0))+delta*(9 if moving else 2)
		hero_visual.set_meta("phase",phase)
		for key in hero_parts:
			var part: Node3D = hero_parts[key]
			part.rotation.x = sin(phase+(PI if str(key).begins_with("leg") else 0))* (0.42 if moving else 0.04)
	if is_instance_valid(hammer):
		if attack_cd > 0.2:
			hammer.rotation.x = lerpf(hammer.rotation.x,-0.75,delta*18)
		else:
			hammer.rotation.x = lerpf(hammer.rotation.x,0,delta*12)

func _press_dir(dir: Vector2) -> void:
	held[str(dir)] = dir
	_recalc_move()

func _release_dir(dir: Vector2) -> void:
	held.erase(str(dir))
	_recalc_move()

func _recalc_move() -> void:
	move = Vector2.ZERO
	for dir in held.values():
		move += dir
	if move.length() > 1:
		move = move.normalized()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey:
		var k := event as InputEventKey
		if k.keycode in [KEY_W,KEY_UP]:
			move.y = -1 if k.pressed else 0
		elif k.keycode in [KEY_S,KEY_DOWN]:
			move.y = 1 if k.pressed else 0
		elif k.keycode in [KEY_A,KEY_LEFT]:
			move.x = -1 if k.pressed else 0
		elif k.keycode in [KEY_D,KEY_RIGHT]:
			move.x = 1 if k.pressed else 0
		elif k.pressed and k.keycode == KEY_SPACE:
			_attack()
		elif k.pressed and k.keycode == KEY_SHIFT:
			_dodge()
		elif k.pressed and k.keycode == KEY_E:
			_interact()

func _show_message(text_value: String) -> void:
	message.text = text_value
	message_cd = 2.7

func _open_upgrade_menu() -> void:
	if not active or ended or get_tree().paused:
		return
	get_tree().paused = true
	upgrade_panel = PanelContainer.new()
	upgrade_panel.set_anchors_preset(Control.PRESET_CENTER)
	upgrade_panel.position = Vector2(-205,-175)
	upgrade_panel.custom_minimum_size = Vector2(410,350)
	hud.add_child(upgrade_panel)
	var box := VBoxContainer.new()
	upgrade_panel.add_child(box)
	var title := Label.new()
	title.text = "کارگاه ارتقای کاوه"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size",26)
	box.add_child(title)
	var balance := Label.new()
	balance.name = "Balance"
	balance.text = "سکه‌های تو: %d" % coins
	balance.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	box.add_child(balance)
	var hammer_upgrade := Button.new()
	hammer_upgrade.text = "تقویت چکش  |  ۵۰ سکه  |  +۸ قدرت ضربه"
	hammer_upgrade.custom_minimum_size.y = 54
	hammer_upgrade.pressed.connect(func():
		if coins < 50:
			balance.text = "سکه کافی نداری؛ با شکست دشمنان سکه بگیر."
			return
		coins -= 50
		attack_damage += 8.0
		_update_counts()
		_save(chapter)
		balance.text = "قدرت ضربه افزایش یافت! قدرت فعلی: %d" % int(attack_damage)
		hammer_upgrade.disabled = true
	)
	box.add_child(hammer_upgrade)
	var armor_upgrade := Button.new()
	armor_upgrade.text = "تقویت زره  |  ۴۰ سکه  |  +۲۰ سلامتی"
	armor_upgrade.custom_minimum_size.y = 54
	armor_upgrade.pressed.connect(func():
		if coins < 40:
			balance.text = "سکه کافی نداری؛ با شکست دشمنان سکه بگیر."
			return
		coins -= 40
		max_health += 20.0
		health = minf(max_health,health+20.0)
		hp_bar.max_value = max_health
		hp_bar.value = health
		_update_counts()
		_save(chapter)
		balance.text = "زره تقویت شد! سلامتی بیشینه: %d" % int(max_health)
		armor_upgrade.disabled = true
	)
	box.add_child(armor_upgrade)
	var close := Button.new()
	close.text = "بازگشت به نبرد"
	close.custom_minimum_size.y = 48
	close.pressed.connect(_close_upgrade_menu)
	box.add_child(close)

func _close_upgrade_menu() -> void:
	get_tree().paused = false
	if is_instance_valid(upgrade_panel):
		upgrade_panel.queue_free()
	upgrade_panel = null

func _toggle_pause() -> void:
	if not active or ended:
		return
	if get_tree().paused:
		get_tree().paused = false
		if is_instance_valid(pause_panel):
			pause_panel.queue_free()
		pause_panel = null
		return
	_save(chapter)
	get_tree().paused = true
	pause_panel = PanelContainer.new()
	pause_panel.set_anchors_preset(Control.PRESET_CENTER)
	pause_panel.position = Vector2(-190,-170)
	pause_panel.custom_minimum_size = Vector2(380,340)
	hud.add_child(pause_panel)
	var box := VBoxContainer.new()
	pause_panel.add_child(box)
	var title := Label.new()
	title.text = "بازی متوقف شد"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size",28)
	box.add_child(title)
	var volume_label := Label.new()
	volume_label.text = "بلندی افکت‌های صوتی"
	box.add_child(volume_label)
	var volume := HSlider.new()
	volume.min_value = 0.0
	volume.max_value = 1.0
	volume.step = 0.05
	volume.value = sfx_volume
	volume.value_changed.connect(_set_sfx_volume)
	box.add_child(volume)
	var resume := Button.new()
	resume.text = "ادامه بازی"
	resume.custom_minimum_size.y = 48
	resume.pressed.connect(_toggle_pause)
	box.add_child(resume)
	var menu := Button.new()
	menu.text = "ذخیره و بازگشت به منو"
	menu.custom_minimum_size.y = 48
	menu.pressed.connect(func():
		get_tree().paused = false
		pause_panel.queue_free()
		pause_panel = null
		active = false
		_show_title()
	)
	box.add_child(menu)

func _set_sfx_volume(value: float) -> void:
	sfx_volume = clampf(value,0.0,1.0)
	for player_sfx in sfx_pool:
		player_sfx.volume_db = linear_to_db(maxf(0.001,sfx_volume))
	_save(chapter)

func _save(next_chapter: int) -> void:
	saved_chapter = clampi(next_chapter,0,Chapters.chapter_count()-1)
	var cfg := ConfigFile.new()
	cfg.set_value("campaign","chapter",saved_chapter)
	cfg.set_value("campaign","coins",coins)
	cfg.save("user://progress.cfg")

func _load_save() -> void:
	var cfg := ConfigFile.new()
	if cfg.load("user://progress.cfg") == OK:
		saved_chapter = clampi(int(cfg.get_value("campaign","chapter",0)),0,Chapters.chapter_count()-1)
		coins = int(cfg.get_value("campaign","coins",0))
		sfx_volume = clampf(float(cfg.get_value("settings","sfx_volume",0.8)),0.0,1.0)
		attack_damage = maxf(DAMAGE,float(cfg.get_value("upgrades","attack_damage",DAMAGE)))
		max_health = maxf(MAX_HP,float(cfg.get_value("upgrades","max_health",MAX_HP)))
		health = max_health

func _clear_enemies() -> void:
	for enemy in enemies:
		if is_instance_valid(enemy):
			enemy.queue_free()
	enemies.clear()

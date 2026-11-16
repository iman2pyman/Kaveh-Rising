extends Node3D

const MOVE_SPEED := 5.2
const ENEMY_SPEED := 2.0
const ATTACK_RANGE := 2.3
const MAX_HEALTH := 100.0

var player: CharacterBody3D
var enemy: CharacterBody3D
var camera: Camera3D
var health := MAX_HEALTH
var enemy_health := 100.0
var attack_cooldown := 0.0
var message_time := 0.0
var move_vector := Vector2.ZERO
var health_bar: ProgressBar
var enemy_bar: ProgressBar
var status_label: Label

func _ready() -> void:
	_build_world()
	_build_player()
	_build_enemy()
	_build_camera()
	_build_hud()
	_show_message("فصل اول: آتش آهنگری — قیام آغاز می‌شود")

func _build_world() -> void:
	var world_env := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.sky = Sky.new()
	env.sky.sky_material = ProceduralSkyMaterial.new()
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.55, 0.58, 0.68)
	env.ambient_light_energy = 0.7
	world_env.environment = env
	add_child(world_env)

	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -32, 0)
	sun.light_energy = 1.15
	add_child(sun)

	var ground := StaticBody3D.new()
	var ground_mesh := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(80, 80)
	ground_mesh.mesh = plane
	var ground_mat := StandardMaterial3D.new()
	ground_mat.albedo_color = Color(0.19, 0.22, 0.17)
	ground_mesh.material_override = ground_mat
	ground.add_child(ground_mesh)
	var ground_collision := CollisionShape3D.new()
	var ground_shape := BoxShape3D.new()
	ground_shape.size = Vector3(80, 0.2, 80)
	ground_collision.shape = ground_shape
	ground_collision.position.y = -0.1
	ground.add_child(ground_collision)
	add_child(ground)

	for i in range(12):
		var rock := MeshInstance3D.new()
		var mesh := SphereMesh.new()
		mesh.radius = randf_range(0.5, 1.3)
		mesh.height = mesh.radius * 1.4
		rock.mesh = mesh
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(0.22, 0.23, 0.24)
		rock.material_override = mat
		rock.position = Vector3(randf_range(-13, 13), 0.2, randf_range(-12, 12))
		add_child(rock)

func _make_character(body: CharacterBody3D, color: Color, height: float) -> void:
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.48
	capsule.height = height
	var visual := MeshInstance3D.new()
	visual.mesh = capsule
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = 0.82
	visual.material_override = mat
	visual.position.y = height * 0.5
	body.add_child(visual)
	var collision := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.48
	shape.height = height
	collision.shape = shape
	collision.position.y = height * 0.5
	body.add_child(collision)

func _build_player() -> void:
	player = CharacterBody3D.new()
	player.name = "Kaveh"
	_make_character(player, Color(0.72, 0.25, 0.12), 1.9)
	player.position = Vector3(0, 0, 3)
	add_child(player)

func _build_enemy() -> void:
	enemy = CharacterBody3D.new()
	enemy.name = "ZahhakGuard"
	_make_character(enemy, Color(0.12, 0.16, 0.22), 2.0)
	enemy.position = Vector3(0, 0, -5)
	add_child(enemy)

func _build_camera() -> void:
	camera = Camera3D.new()
	camera.position = Vector3(0, 5.5, 9)
	camera.rotation_degrees = Vector3(-25, 0, 0)
	camera.current = true
	player.add_child(camera)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(18, 16)
	panel.custom_minimum_size = Vector2(270, 92)
	layer.add_child(panel)
	var column := VBoxContainer.new()
	panel.add_child(column)
	var title := Label.new()
	title.text = "قیام کاوه  |  فصل اول"
	title.add_theme_font_size_override("font_size", 22)
	column.add_child(title)
	health_bar = ProgressBar.new()
	health_bar.max_value = MAX_HEALTH
	health_bar.value = health
	health_bar.custom_minimum_size = Vector2(245, 18)
	column.add_child(health_bar)
	enemy_bar = ProgressBar.new()
	enemy_bar.max_value = 100
	enemy_bar.value = enemy_health
	enemy_bar.custom_minimum_size = Vector2(245, 12)
	column.add_child(enemy_bar)

	status_label = Label.new()
	status_label.text = "نگهبان ضحاک را شکست بده"
	status_label.position = Vector2(18, 120)
	status_label.add_theme_font_size_override("font_size", 20)
	layer.add_child(status_label)

	var controls := HBoxContainer.new()
	controls.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT)
	controls.position = Vector2(-350, -112)
	controls.size = Vector2(330, 82)
	layer.add_child(controls)
	var symbols := ["◀", "▲", "▼", "▶"]
	var directions := [Vector2(-1, 0), Vector2(0, -1), Vector2(0, 1), Vector2(1, 0)]
	for i in range(symbols.size()):
		var button := Button.new()
		button.text = symbols[i]
		button.custom_minimum_size = Vector2(68, 72)
		button.add_theme_font_size_override("font_size", 26)
		button.button_down.connect(_set_direction.bind(directions[i]))
		button.button_up.connect(_clear_direction)
		controls.add_child(button)

	var attack_button := Button.new()
	attack_button.text = "ضربه"
	attack_button.set_anchors_preset(Control.PRESET_BOTTOM_LEFT)
	attack_button.position = Vector2(24, -112)
	attack_button.size = Vector2(118, 82)
	attack_button.add_theme_font_size_override("font_size", 24)
	attack_button.pressed.connect(_attack)
	layer.add_child(attack_button)

func _set_direction(direction: Vector2) -> void:
	move_vector = direction

func _clear_direction() -> void:
	move_vector = Vector2.ZERO

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed:
		match event.keycode:
			KEY_W, KEY_UP: move_vector.y = -1
			KEY_S, KEY_DOWN: move_vector.y = 1
			KEY_A, KEY_LEFT: move_vector.x = -1
			KEY_D, KEY_RIGHT: move_vector.x = 1
			KEY_SPACE: _attack()
	elif event is InputEventKey and not event.pressed:
		if event.keycode in [KEY_W, KEY_UP, KEY_S, KEY_DOWN]: move_vector.y = 0
		if event.keycode in [KEY_A, KEY_LEFT, KEY_D, KEY_RIGHT]: move_vector.x = 0

func _physics_process(delta: float) -> void:
	if health <= 0 or enemy_health <= 0:
		return
	attack_cooldown = maxf(0.0, attack_cooldown - delta)
	var direction := Vector3(move_vector.x, 0, move_vector.y)
	if direction.length() > 1.0:
		direction = direction.normalized()
	player.velocity.x = direction.x * MOVE_SPEED
	player.velocity.z = direction.z * MOVE_SPEED
	if direction.length() > 0.05:
		player.rotation.y = atan2(-direction.x, -direction.z)
	player.velocity.y -= 20.0 * delta
	player.move_and_slide()

	var to_player := player.global_position - enemy.global_position
	to_player.y = 0
	if to_player.length() > 1.65:
		var chase := to_player.normalized()
		enemy.velocity.x = chase.x * ENEMY_SPEED
		enemy.velocity.z = chase.z * ENEMY_SPEED
		enemy.rotation.y = atan2(-chase.x, -chase.z)
	else:
		enemy.velocity.x = 0
		enemy.velocity.z = 0
		if randf() < delta * 0.65:
			health = maxf(0, health - 5)
			health_bar.value = health
			_show_message("نگهبان حمله کرد! مراقب باش")
	enemy.velocity.y -= 20.0 * delta
	enemy.move_and_slide()
	camera.look_at(player.global_position + Vector3(0, 0.8, -1.5))
	if message_time > 0:
		message_time -= delta
		if message_time <= 0 and enemy_health > 0:
			status_label.text = "نگهبان ضحاک را شکست بده"

func _attack() -> void:
	if attack_cooldown > 0 or enemy_health <= 0 or health <= 0:
		return
	attack_cooldown = 0.55
	if player.global_position.distance_to(enemy.global_position) <= ATTACK_RANGE:
		enemy_health = maxf(0, enemy_health - 25)
		enemy_bar.value = enemy_health
		_show_message("ضربه کاری کاوه!")
		if enemy_health <= 0:
			_show_message("پیروزی! راه خیزش مردم باز شد")
	else:
		_show_message("برای ضربه به دشمن نزدیک‌تر شو")

func _show_message(message: String) -> void:
	status_label.text = message
	message_time = 3.0

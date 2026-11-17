extends Control

signal vector_changed(value: Vector2)

var active_pointer := -1
var knob := Vector2.ZERO
var radius := 57.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	queue_redraw()

func _gui_input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		if event.pressed and active_pointer == -1:
			active_pointer = event.index
			_update_stick(event.position)
		elif not event.pressed and event.index == active_pointer:
			active_pointer = -1
			knob = Vector2.ZERO
			vector_changed.emit(Vector2.ZERO)
			queue_redraw()
	elif event is InputEventScreenDrag and event.index == active_pointer:
		_update_stick(event.position)
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed and active_pointer == -1:
			active_pointer = 0
			_update_stick(event.position)
		elif not event.pressed and active_pointer == 0:
			active_pointer = -1
			knob = Vector2.ZERO
			vector_changed.emit(Vector2.ZERO)
			queue_redraw()
	elif event is InputEventMouseMotion and active_pointer == 0:
		_update_stick(event.position)

func _update_stick(point: Vector2) -> void:
	var center := size * 0.5
	var offset := point - center
	if offset.length() > radius:
		offset = offset.normalized() * radius
	knob = offset
	var direction := offset / radius
	if direction.length() < 0.13:
		direction = Vector2.ZERO
	vector_changed.emit(direction)
	queue_redraw()

func _draw() -> void:
	var center := size * 0.5
	draw_circle(center,75.0,Color(0.055,0.045,0.04,0.62))
	draw_arc(center,75.0,0.0,TAU,64,Color(0.78,0.58,0.28,0.92),3.0,true)
	draw_circle(center,57.0,Color(0.18,0.13,0.09,0.7))
	draw_circle(center+knob,29.0,Color(0.72,0.51,0.25,0.95))
	draw_arc(center+knob,29.0,0.0,TAU,48,Color(0.96,0.82,0.53,1.0),2.0,true)

extends Node2D


@onready var rtl: RichTextLabel = $CanvasLayer/PanelContainer/VBoxContainer/RichTextLabel
@onready var opt: OptionButton = $CanvasLayer/PanelContainer/VBoxContainer/HBoxContainer/OptionButton
@onready var chat: Node = $Chat
@onready var edit: LineEdit = $CanvasLayer/PanelContainer/VBoxContainer/HBoxContainer/LineEdit


func _ready() -> void:
	edit.text_submitted.connect(
		func (text: String) -> void:
			var _chat := get_chat()
			_chat.ask(text)
			edit.text = ""
			
	)
	
	for node: Node in chat.get_children():
		if node is NobodyWhoChat:
			node.response_finished.connect(
				func (response: String) -> void:
					rtl.text += "\n"
					rtl.text += response
			)
			node.start_worker()


func get_chat() -> NobodyWhoChat:
	return get_node(NodePath("Chat/" + opt.text)) as NobodyWhoChat

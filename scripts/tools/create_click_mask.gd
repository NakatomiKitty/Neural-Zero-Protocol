@tool
class_name _CreateClickMask extends EditorScript


func _run() -> void:
	print()
	var selected_paths := EditorInterface.get_selected_paths()
	if selected_paths.is_empty():
		printerr("Select one or more PNG images in the file system dock!")
		return
	
	for f: String in selected_paths:
		if f.ends_with(".png") and FileAccess.file_exists(f):
			var image := ResourceLoader.load(f)
			assert(image is Texture2D, "Loaded resource isn't of type Texture2D, but %s" % image.get_class())
			var bm := BitMap.new()
			bm.create_from_image_alpha(image.get_image())
			var destination := f.replace(".png", "_click_mask.tres")
			var save_error := ResourceSaver.save(bm, destination)
			if save_error == OK:
				print_rich("[color=forestgreen]OK[/color] Created click mask for", f.get_basename(), "at", destination)
			else:
				printerr("Failed to save click mask resource: %s" % error_string(save_error))
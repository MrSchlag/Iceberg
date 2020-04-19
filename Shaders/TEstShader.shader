shader_type canvas_item;

void vertex()
{
	COLOR = vec4(VERTEX.x * VERTEX.y, 0.0, 0.0, 1.0);
}

void fragment()
{
	//COLOR = vec4(UV.x, 0.0, 0.0, 1.0);
}
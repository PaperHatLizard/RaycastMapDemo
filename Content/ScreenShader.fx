#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif



float2 ScreenSize;
float2 MapSize;
float2 PlayerPosition;
float PlayerRotation;

float2 MapSamplePoint;

float4 MapValue;

Texture2D SpriteTexture;
texture2D MapTexture;

sampler MapSampler : register(s1)
{
    Texture = <MapTexture>;
    MinFilter = None;
    MagFilter = None;
    MipFilter = None;
};
sampler SpriteTextureSampler : register(s0)
{
    Texture = <SpriteTexture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct Line
{
    float2 Start;
    float2 End;
};

float2 Bresenham(float x0, float y0, float angle);

float2 MapToUVCoords(float2 mapCoords);

int ShouldDrawWall(int row, int column);

float DistanceFromLineIntersection(Line startLine, float2 cellPosition, int lastSideHit);

float Frac(float value);

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color;
    
    float4 mapValue = tex2D(MapSampler, MapToUVCoords(MapSamplePoint));
    
    float4 ceilingColor = float4(0.0, 0.33, 0.68, 1.0);
    float4 floorColor = float4(0.02, 0.04, 0.05, 1.0);

    // Calculate the coordinates of the screen
    int column = (input.TextureCoordinates.x * ScreenSize.x);
    int row = (input.TextureCoordinates.y * ScreenSize.y);
    
    int drawWall = ShouldDrawWall(column, row);

    // Set initial color to ceiling or floor color
    color = row < ScreenSize.y / 2 ? ceilingColor : floorColor;
    
    // Draw wall according to face side
    if (drawWall >= 1)
    {
        //X+
        if (drawWall == 1)
            color = float4(0, 0, 1, 0);
        //X-
        else if (drawWall == 2)
            color = float4(0, 1, 0, 0);
        //Y+
        else if (drawWall == 3)
            color = float4(1, 0, 0, 0);
        //Y-
        else if (drawWall == 4)
            color = float4(0, 1, 1, 0);

    }
    
    if (drawWall == -2)
    {
        color = float4(1, 1, 0, 1);
    }
    
    color.a = 1.0;
    
    return color;
}

int ShouldDrawWall(int column, int row)
{
    float wallHeight = 1;
    //range -1 to 1
    float angleScale = ((float(column) / float(ScreenSize.x)) - 0.5) * 2;
    
    float fov = radians(40);
    
    float angle = PlayerRotation + (angleScale * fov);
    
    float2 lineStart = float2(PlayerPosition.x, PlayerPosition.y);
    
    float2 sideAndDistance = Bresenham(lineStart.x, lineStart.y, angle);
    
    float distance = sideAndDistance.y;
    
    //Adjust for fish eye effect
    distance *= cos(PlayerRotation - angle);
    
    float adjustedWallHeight = wallHeight * ScreenSize.y / distance;
    float wallStart = (ScreenSize.y / 2.0) - (adjustedWallHeight / 2.0);
    float wallEnd = (ScreenSize.y / 2.0) + (adjustedWallHeight / 2.0);

    bool isWall = (row >= wallStart && row <= wallEnd);

    if (distance < 0)
    {
        if (isWall)
            return -2;
    }
    
    return isWall ? sideAndDistance.x : 0;
}


float2 Bresenham(float x0, float y0, float angle)
{
    float dx = cos(angle);
    float dy = sin(angle);
    
    float xStep = sign(dx);
    float yStep = sign(dy);

    float xPos = x0;
    float yPos = y0;
    
    float tMaxX = xStep > 0 ? (1.0 - Frac(xPos)) / dx : Frac(xPos) / -dx;
    float tMaxY = yStep > 0 ? (1.0 - Frac(yPos)) / dy : Frac(yPos) / -dy;

    float tDeltaX = 1.0 / abs(dx);
    float tDeltaY = 1.0 / abs(dy);
    
    int maxRayLength = 30;
    float lastSideHit = -1;

    [unroll(maxRayLength)]
    for (int i = 0; i < maxRayLength; i++)
    {
        //clamp position for float imprecision
        float xMap = floor(xPos * 1000 + 0.5) / 1000; 
        float yMap = floor(yPos * 1000 + 0.5) / 1000;
        
        float4 mapValue = tex2D(MapSampler, MapToUVCoords(float2(xMap, yMap)));
        
        if (mapValue.r != 1 && mapValue.g != 1 && mapValue.b != 1)
        {
            if (lastSideHit == -1)
                lastSideHit == 1;
            
            Line startLine = { 
                float2(x0, y0), 
                float2(x0, y0) + float2(cos(angle), sin(angle)) * maxRayLength
            };
            
            float distance = DistanceFromLineIntersection(startLine, float2(xPos, yPos), lastSideHit);
            
            if (distance == -1)
                return float2(-1, -1);
            
            return float2(lastSideHit, distance);
        }

        if (tMaxX < tMaxY)
        {
            lastSideHit = dx > 0 ? 1 : 2;
            tMaxX += tDeltaX;
            xPos += xStep;
        }
        else
        {
            lastSideHit = dy > 0 ? 3 : 4;
            tMaxY += tDeltaY;
            yPos += yStep;
        }
    }
    
    return float2(-1, -1);
}


float DistanceFromLineIntersection(Line startLine, float2 cellPosition, int lastSideHit)
{
    Line intersectLine;
    
    cellPosition.x = floor(cellPosition.x) + 0.5;
    cellPosition.y = floor(cellPosition.y) + 0.5;
    
    float cellSize = 0.5;
    
    if (lastSideHit == 1)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x - cellSize, cellPosition.y + cellSize);
    }
    else if (lastSideHit == 2)
    {
        intersectLine.Start = float2(cellPosition.x + cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y + cellSize);
    }
    else if (lastSideHit == 3)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y - cellSize);
    }
    else if (lastSideHit == 4)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y + cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y + cellSize);
    }
    else
        return -1;
    
    float m1 = (startLine.End.y - startLine.Start.y) / (startLine.End.x - startLine.Start.x + 0.00001);
    float m2 = (intersectLine.End.y - intersectLine.Start.y) / (intersectLine.End.x - intersectLine.Start.x + 0.00001);
    
    //If slopes are nearly identical then these lines probably dont intersect
    if (abs(m1 - m2) < 0.0001)
    {
        return -1;
    }
    
    float b1 = startLine.Start.y - m1 * startLine.Start.x;
    float b2 = intersectLine.Start.y - m2 * intersectLine.Start.x;
    
    float x = (b2 - b1) / (m1 - m2);
    float y = m1 * x + b1;
    
    float2 intersection = float2(x, y);
    
    return distance(startLine.Start, intersection);
}

float Frac(float value)
{
    return value - floor(value);
}



float2 MapToUVCoords(float2 mapCoords)
{
    //For some reason epsi is needed to prevent the texture
    //from being sampled at the wrong position
    float x = (float(mapCoords.x) / float(MapSize.x));
    float y = (float(mapCoords.y) / float(MapSize.y));
    float epsi = 0.0001;
    
    return float2(x + epsi, y + epsi);
}


technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
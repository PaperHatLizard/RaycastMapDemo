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
Texture2D WallTexture;

sampler SpriteTextureSampler : register(s0)
{
    Texture = <SpriteTexture>;
    MinFilter = None;
    MagFilter = None;
    MipFilter = None;
};
sampler MapSampler : register(s1)
{
    Texture = <MapTexture>;
    MinFilter = None;
    MagFilter = None;
    MipFilter = None;
};
sampler WallTextureSampler
{
    Texture = <WallTexture>;
    MinFilter = None;
    MagFilter = None;
    MipFilter = None;
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

float3 Bresenham(float x0, float y0, float angle);

float2 MapToUVCoords(float2 mapCoords);

float4 ShouldDrawWall(int row, int column);

float2 DistanceFromLineIntersection(Line startLine, float2 cellPosition, int lastSideHit);

float Frac(float value);

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color;
    
    float4 mapValue = tex2D(MapSampler, MapToUVCoords(MapSamplePoint));
    
    float4 ceilingColor = float4(0.0, 0.43, 0.7, 1.0);
    float4 floorColor = float4(0.02, 0.04, 0.05, 1.0);
    float4 fogColor = float4(0.0, 0.0, 0.0, 1.0);

    // Calculate the coordinates of the screen
    int column = (input.TextureCoordinates.x * ScreenSize.x);
    int row = (input.TextureCoordinates.y * ScreenSize.y);
    
    float4 drawWall = ShouldDrawWall(column, row);
    
    float xCoord = drawWall.x;
    float yCoord = drawWall.y;
    float side = drawWall.z;
    float distance = drawWall.a;

    // Set initial color to ceiling or floor color
    color = row < ScreenSize.y / 2 ? ceilingColor : floorColor;
    
    if (drawWall.a != 0)
    {
        float2 textureCoords = float2(drawWall.x, drawWall.y);
        color = tex2D(WallTextureSampler, textureCoords);
        color = lerp(color, fogColor, distance / 10.0);
    }
    
    color.a = 1.0;
    
    return color;
}

float4 ShouldDrawWall(int column, int row)
{
    float wallHeight = 1;
    //range -1 to 1
    float angleScale = ((float(column) / float(ScreenSize.x)) - 0.5) * 2;
    
    float fov = radians(40);
    
    float angle = PlayerRotation + (angleScale * fov);
    
    float2 lineStart = float2(PlayerPosition.x, PlayerPosition.y);
    
    float3 bresenhamCalc = Bresenham(lineStart.x, lineStart.y, angle);
    
    float side = bresenhamCalc.x;
    
    float distance = bresenhamCalc.y;
    
    float coord = bresenhamCalc.z;
    
    //Adjust for fish eye effect
    distance *= cos(PlayerRotation - angle);
    
    Line distanceLine =
    {
        lineStart,
        lineStart + float2(cos(angle), sin(angle)) * distance
    };
    
    float2 mapTexCoords = lineStart + float2(cos(angle), sin(angle)) * (distance * 1.1);
    
    
    float adjustedWallHeight = wallHeight * ScreenSize.y / distance;
    float wallStart = (ScreenSize.y / 2.0) - (adjustedWallHeight / 2.0);
    float wallEnd = (ScreenSize.y / 2.0) + (adjustedWallHeight / 2.0);

    float yCoord = wallEnd - wallStart;
    
    yCoord = (row - wallStart) / yCoord;
    
    
    float2 TextureCoords = float2(coord, yCoord);
    
    
    
    bool isWall = (row >= wallStart && row <= wallEnd);

    if (distance < 0 || !isWall)
    {
        return float4(0, 0, 0, 0);
    }
    
    
    return float4(TextureCoords, side, distance);

}


float3 Bresenham(float x0, float y0, float angle)
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
        
        if (mapValue.r < 0.99 && mapValue.g < 0.99 && mapValue.b < 0.99)
        {
            if (lastSideHit == -1)
                lastSideHit = 1;
            
            Line startLine = { 
                float2(x0, y0), 
                float2(x0, y0) + float2(cos(angle), sin(angle)) * maxRayLength
            };
            
            float2 distanceAndCoord = DistanceFromLineIntersection(startLine, float2(xPos, yPos), lastSideHit);
            
            if (distanceAndCoord.x == -1)
                return float3(-1, -1, -1);
            
            return float3(lastSideHit, distanceAndCoord);
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
    
    return float3(-1, -1, -1);
}

float2 DistanceFromLineIntersection(Line startLine, float2 cellPosition, int lastSideHit)
{
    Line intersectLine;
    
    cellPosition.x = floor(cellPosition.x) + 0.5;
    cellPosition.y = floor(cellPosition.y) + 0.5;
    
    float cellSize = 0.5;
    
    bool isX;
    
    //X-
    if (lastSideHit == 1)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x - cellSize, cellPosition.y + cellSize);
        
        isX = true;
    }
    //X+
    else if (lastSideHit == 2)
    {
        intersectLine.Start = float2(cellPosition.x + cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y + cellSize);
        
        isX = true;
    }
    //Y-
    else if (lastSideHit == 3)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y - cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y - cellSize);
        
        isX = false;
    }
    //Y+
    else if (lastSideHit == 4)
    {
        intersectLine.Start = float2(cellPosition.x - cellSize, cellPosition.y + cellSize);
        intersectLine.End = float2(cellPosition.x + cellSize, cellPosition.y + cellSize);
        
        isX = false;
    }
    else
        return float2(-1, -1);

    
    float m1 = (startLine.End.y - startLine.Start.y) / (startLine.End.x - startLine.Start.x + 0.00001);
    float m2 = (intersectLine.End.y - intersectLine.Start.y) / (intersectLine.End.x - intersectLine.Start.x + 0.00001);
    
    //If slopes are nearly identical then these lines probably dont intersect
    if (abs(m1 - m2) < 0.0001)
    {
        return float2(-1, -1);
    }
    
    float b1 = startLine.Start.y - m1 * startLine.Start.x;
    float b2 = intersectLine.Start.y - m2 * intersectLine.Start.x;
    
    float x = (b2 - b1) / (m1 - m2);
    float y = m1 * x + b1;
    
    float2 intersection = float2(x, y);
    
    float distToIntersect = distance(intersectLine.Start, intersection);
    float distToEnd = distance(intersectLine.Start, intersectLine.End);
    
    float delta = distToIntersect / distToEnd;
    
    
    
    //Return distance of line and intersection coordinate
    return float2(distance(startLine.Start, intersection), delta);
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
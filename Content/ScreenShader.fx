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
int WallTexturesCount;

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
    float offset = drawWall.z;
    float distance = drawWall.a;

    // Set initial color to ceiling or floor color
    color = row < ScreenSize.y / 2 ? ceilingColor : floorColor;
    
    //If a wall exists at this coordinate
    if (distance != 0)
    {   
        float drawWallWidth = 1.0 / (WallTexturesCount);
        
        float adjustedxCoord = (drawWallWidth * xCoord) + offset;
            
        float2 textureCoords = float2(adjustedxCoord, yCoord);
        
        
        //textureCoords.x += offset / drawWallWidth;
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
    
    float xCoord = bresenhamCalc.z;
    
    float2 mapTexCoords = lineStart + float2(cos(angle), sin(angle)) * (distance+0.1f);
    
    //Adjust for fish eye effect
    distance *= cos(PlayerRotation - angle);
    
    Line distanceLine =
    {
        lineStart,
        lineStart + float2(cos(angle), sin(angle)) * distance
    };
    
    
    float adjustedWallHeight = wallHeight * ScreenSize.y / distance;
    float wallStart = (ScreenSize.y / 2.0) - (adjustedWallHeight / 2.0);
    float wallEnd = (ScreenSize.y / 2.0) + (adjustedWallHeight / 2.0);

    float yCoord = wallEnd - wallStart;
    
    yCoord = (row - wallStart) / yCoord;
    
    //Round to 2 decimal places to prevent noisy textures
    yCoord = round(yCoord * 100) / 100;
    
    
    float2 TextureCoords = float2(xCoord, yCoord);
    
    //Calculate texture offset in the texture atlas using red channel of map texture
    float offset = tex2D(MapSampler, MapToUVCoords(mapTexCoords)).r;
    
    bool isWall = (row >= wallStart && row <= wallEnd);

    //Don't draw wall if dist is negative or if the wall is not in the range
    if (distance < 0 || !isWall)
    {
        return float4(0, 0, 0, 0);
    }
    
    
    return float4(TextureCoords, offset, distance);

}

//Bresenham's line algorithm for raycasting all pixels
//Algorithm based off this stackO answer: https://stackoverflow.com/a/12934943/28842751
float3 Bresenham(float x0, float y0, float angle)
{
    float dx = cos(angle);
    float dy = sin(angle);
    
    float xStep = sign(dx);
    float yStep = sign(dy);

    float xPos = x0;
    float yPos = y0;
    
    float tMaxX = xStep > 0 ? (1.0 - frac(xPos)) / dx : frac(xPos) / -dx;
    float tMaxY = yStep > 0 ? (1.0 - frac(yPos)) / dy : frac(yPos) / -dy;

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
        
        //If the texture isn't white then this is a wall
        if (mapValue.r < 0.99 || mapValue.g < 0.99 || mapValue.b < 0.99)
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

        //If we didn't find a wall yet, continue the ray
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
    
    //No wall found, return -1
    return float3(-1, -1, -1);
}

float2 DistanceFromLineIntersection(Line startLine, float2 cellPosition, int lastSideHit)
{
    Line intersectLine;
    
    //Center the cell position to make intersection line
    cellPosition.x = floor(cellPosition.x) + 0.5;
    cellPosition.y = floor(cellPosition.y) + 0.5;
    
    float cellSize = 0.5;
    
    bool isX;
    
    //Check which side was hit last and create the "box" line from that side
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
    //This should never happen? Unless the ray somehow hits a wall before the for loop iterates once
    else
        return float2(-1, -1);

    //Intersection algorithm based off https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
    
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
    
    //Calculate the distance from the start of the line to the intersection,
    //this is our x coordinate in the texture
    float delta = distToIntersect / distToEnd;
    
    
    
    //Return distance to the wall as well as our delta, x coordinate in the texture
    return float2(distance(startLine.Start, intersection), delta);
}

float2 MapToUVCoords(float2 mapCoords)
{
    //For some reason epsi is needed to prevent the texture
    //from being sampled at the wrong position and jittery/disappearing walls
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
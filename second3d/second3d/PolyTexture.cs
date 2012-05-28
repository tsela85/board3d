using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace second3d
{
    class PolyTexture
    {
        public struct DividingVert {
            public float distance;
            public int small;
            public int big;
            public Vector3 position;
        }
        private VertexPositionNormalTexture[] vertices;
        private short[] indices;
        int vertNum;      
        private Texture2D texture;
        private Vector3 pointOnEdge;
        private DividingVert[] p;
        public int temp = 0;
        VertexPositionTexture[] ver;
        public PolyTexture one, two;
        public float angle = 0;
        Effect effect;
        Camera camera;
        Matrix worldMatrix;
        GraphicsDevice device;
        InputHandler input;

#region properties

        public VertexPositionNormalTexture[] Vertices
        {
            get{ return vertices;}
            set {vertices = value;}
        }

        public short[] Indices
        {
            get{ return indices;}
            set {indices = value;}
        }
 
#endregion
        
        public void Initialize(Texture2D tex, int vNum, Vector3[] points,Vector2[] texCords,Effect eff,
            ref Camera cam, ref GraphicsDevice dev,ref InputHandler inp)
        {
            pointOnEdge = Vector3.Zero;
            int iCount = (vNum - 3) * 3 + 3;
            vertNum = vNum;
            texture = tex;
            vertices = new VertexPositionNormalTexture[vNum];
            effect = eff;
            camera = cam;
            device = dev;
            input = inp;
            worldMatrix = Matrix.Identity;            

            indices = new short[iCount];
            for (int i = 0; i < vNum; i++)
            {
                vertices[i].Position = points[i];
                vertices[i].TextureCoordinate = texCords[i];
                vertices[i].Normal = Vector3.Up;
            }
            //create the indices. every 2 consecutive points makes an outer edge (x,y,z) -> xy , yz (not xz)
            short j = 0;
            for (short i = 0; i < iCount; i+=3)
            {
                indices[i] = (short)(j % vNum);
                indices[i+1] = (short)((j + 1) % vNum);
                indices[i + 2] = (short)(((j != 4 ? j : 5) + 2) % vNum);
                j += 2;
            }            
            ver = new VertexPositionTexture[3];
            p = new DividingVert[2];

        }

        public void Draw()
        {
            if (temp >= 2)
            {
                one.foldShape(angle);
                one.Draw();
                two.Draw();
                if (temp == 2)
                    angle -= 0.05f;
                if (temp == 3)
                    angle += 0.02f;
                if (angle < -MathHelper.Pi + 0.05f)
                    temp = 3;                
                if (angle > 0)
                    temp = 2;                  
            } else 
            {
           
                effect.CurrentTechnique = effect.Techniques["TexturedNoShading"];
                effect.Parameters["xWorld"].SetValue(worldMatrix);
                effect.Parameters["xView"].SetValue(camera.View);
                effect.Parameters["xProjection"].SetValue(camera.Projection);
                effect.Parameters["xTexture"].SetValue(texture);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0,
                        vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, ver, 0, 1, VertexPositionNormalTexture.VertexDeclaration);

                }
            }
        }

#region Collision
        public bool collideWithEdge(Vector3 point)
        {
            DividingVert minVert=LineIntersectPoint(vertices[0].Position, vertices[1].Position, point);
            minVert.small = 0;
            minVert.big = 1;
            for (int i = 1; i < vertNum; i++)
            {
                DividingVert currentVert = LineIntersectPoint(vertices[i].Position, vertices[(i+1)%vertNum].Position, point);
                if (currentVert.distance < minVert.distance)
                {
                    minVert = currentVert;
                    minVert.small = i;
                    minVert.big = (i + 1) % vertNum;
                }                
            }
            if (minVert.distance < 1f)
            {
                pointOnEdge = minVert.position;
                ver[0].Position = new Vector3(pointOnEdge.X - 0.5f, 0, pointOnEdge.Z - 0.5f);
                ver[0].TextureCoordinate = Vector2.Zero;
                ver[1].Position = new Vector3(pointOnEdge.X, 0f, pointOnEdge.Z + 0.5f);
                ver[1].TextureCoordinate = Vector2.Zero;
                ver[2].Position = new Vector3(pointOnEdge.X + 0.5f, 0f, pointOnEdge.Z - 0.5f);
                ver[2].TextureCoordinate = Vector2.Zero;
                p[temp] = minVert;
                temp++;
                return true;
            }
            return false;
        }

        private DividingVert LineIntersectPoint(Vector3 v1, Vector3 v2, Vector3 point)
        {
            Vector2 A = new Vector2(v1.X, v1.Z);
            Vector2 B = new Vector2(v2.X, v2.Z);
            Vector2 p = new Vector2(point.X, point.Z);
 
            DividingVert edgePoint = new DividingVert();
            //get the normalized line segment vector
            Vector2 v = B - A;
            v.Normalize();

            //determine the point on the line segment nearest to the point p
            float distanceAlongLine = Vector2.Dot(p, v) - Vector2.Dot(A, v);
            Vector2 nearestPoint;
            if (distanceAlongLine < 0)
            {
                //closest point is A
                nearestPoint = A;
            }
            else if (distanceAlongLine > Vector2.Distance(A, B))
            {
                //closest point is B
                nearestPoint = B;
            }
            else
            {
                //closest point is between A and B... A + d  * ( ||B-A|| )
                nearestPoint = A + distanceAlongLine * v;
            }

            //Calculate the distance between the two points
            float actualDistance = Vector2.Distance(nearestPoint, p);
            edgePoint.distance = actualDistance;
            edgePoint.big = edgePoint.small = -1;
            edgePoint.position = new Vector3(nearestPoint.X, 0, nearestPoint.Y);
            return edgePoint;
        }

        //----------------------------------------------------------------
        // GetPickedPosition() - gets 3D position of mouse pointer
        //                     - always on the the Y = 0 plane     
        //----------------------------------------------------------------

        public Vector3 GetPickedPosition(Vector2 mousePosition)
        {

            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 10 is as far away as possible
            Vector3 nearSource = new Vector3(mousePosition, 0f);
            Vector3 farSource = new Vector3(mousePosition, 1f);

            // find the two screen space positions in world space
            Vector3 nearPoint = device.Viewport.Unproject(nearSource, camera.Projection, camera.View, Matrix.Identity);

            Vector3 farPoint = device.Viewport.Unproject(farSource,
                                camera.Projection, camera.View, Matrix.Identity);

            // compute normalized direction vector from nearPoint to farPoint
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // create a ray using nearPoint as the source
            Ray r = new Ray(nearPoint, direction);

            // calculate the ray-plane intersection point
            Vector3 n = new Vector3(0f, 1f, 0f);
            Plane p = new Plane(n, 0f);

            // calculate distance of intersection point from r.origin
            float denominator = Vector3.Dot(p.Normal, r.Direction);
            float numerator = Vector3.Dot(p.Normal, r.Position) + p.D;
            float t = -(numerator / denominator);

            // calculate the picked position on the y = 0 plane
            Vector3 pickedPosition = nearPoint + direction * t;


            return pickedPosition;
        }
#endregion

#region Divide Shape
        public void Divide(DividingVert first, DividingVert second, out PolyTexture partOne, out PolyTexture partTwo)
        {       
            //
            /* ADD TEST TO FIND IF NOT ON THE SAME EDGE*/
            //
            PolyTexture part1 = new PolyTexture();
            PolyTexture part2 = new PolyTexture();
            int p1_pNum,p2_pNum;            
            p1_pNum = Math.Abs(first.big - second.small) + 3;
            if ((first.big == vertNum - 1) && (second.small == 0))
                p1_pNum -=2;
            p2_pNum = indices.Length - p1_pNum + 2;
            Vector3[] p1_points = new Vector3[p1_pNum];
            Vector2[] p1_texCords = new Vector2[p1_pNum];
            p1_points[0] = first.position;
            p1_texCords[0] = findTexCords(first);
            p1_points[p1_pNum - 1] = second.position;
            p1_texCords[p1_pNum - 1] = findTexCords(second);
            for (int i = 0; i < p1_pNum - 2; i++)
            {
                p1_points[i + 1] = vertices[(first.big + i) % vertNum].Position;
                p1_texCords[i + 1] = vertices[(first.big + i) % vertNum].TextureCoordinate;
            }
            Vector3[] p2_points = new Vector3[p2_pNum];
            Vector2[] p2_texCords = new Vector2[p2_pNum];
            p2_points[0] = second.position;
            p2_texCords[0] = findTexCords(second); //alreadt calculated before a moment
            p2_points[p2_pNum - 1] = first.position;
            p2_texCords[p2_pNum - 1] = findTexCords(first); //alreadt calculated before a moment
            for (int i = 0; i < p2_pNum - 2; i++)
            {
                p2_points[i + 1] = vertices[(second.big + i) % vertNum].Position;
                p2_texCords[i + 1] = vertices[(second.big + i) % vertNum].TextureCoordinate;
            }
            part1.Initialize(texture, p1_pNum, p1_points, p1_texCords,effect,ref camera,ref device,ref input);
            part2.Initialize(texture, p2_pNum, p2_points, p2_texCords, effect, ref camera, ref device,ref input);
            partOne = part1;
            partTwo = part2;
        }

        private Vector2 findTexCords(DividingVert divVert)
        {
            float smallX = vertices[divVert.small].Position.X;
            float smallCordX = vertices[divVert.small].TextureCoordinate.X;
            float bigX = vertices[divVert.big].Position.X;
            float bigCordX = vertices[divVert.big].TextureCoordinate.X;
            float vertX = divVert.position.X;

            float smallZ = vertices[divVert.small].Position.Z;
            float smallCordZ = vertices[divVert.small].TextureCoordinate.Y;
            float bigZ = vertices[divVert.big].Position.Z;
            float bigCordZ = vertices[divVert.big].TextureCoordinate.Y;
            float vertZ = divVert.position.Z;

            float xSize = Math.Abs(bigX - smallX);
            float zSize = Math.Abs(bigZ - smallZ);
            float xRel = (xSize != 0 ? Math.Abs(Math.Min(smallX,bigX) - vertX) / xSize : smallX);
            float zRel = (zSize != 0 ? Math.Abs(Math.Min(smallZ, bigZ) - vertZ) / zSize : smallZ);
            float xCsize = Math.Abs(bigCordX - smallCordX);
            float zCsize = Math.Abs(bigCordZ - smallCordZ);
            float xCrel = (xCsize != 0 ? xRel * xCsize : smallCordX);
            float zCrel = (zCsize != 0 ? zRel * zCsize : smallCordZ);
            return new Vector2(xCrel, zCrel);

            //Vector2 distFromSmall = new Vector2(Math.Abs(divVert.position.X - vertices[divVert.small].Position.X),
            //                       Math.Abs(divVert.position.Z - vertices[divVert.small].Position.Z));
            //Vector2 sizeOfLine = new Vector2(Math.Abs(vertices[divVert.small].Position.X - vertices[divVert.big].Position.X),
            //                Math.Abs(vertices[divVert.small].Position.Z - vertices[divVert.big].Position.Z));
            //distFromSmall /= new Vector2((sizeOfLine.X != 0 ? sizeOfLine.X : 1), (sizeOfLine.Y != 0 ? sizeOfLine.Y : 1));
            //sizeOfLine = new Vector2(Math.Abs(vertices[divVert.small].TextureCoordinate.X - 
            //    vertices[divVert.big].TextureCoordinate.X),
            //    Math.Abs(vertices[divVert.small].TextureCoordinate.Y - vertices[divVert.big].TextureCoordinate.Y));
            //distFromSmall *= sizeOfLine;
            //if (divVert.position.X == vertices[divVert.small].TextureCoordinate.X)
            //    distFromSmall.X = vertices[divVert.small].TextureCoordinate.X;
            //if (vertices[divVert.small].TextureCoordinate.Y == vertices[divVert.big].TextureCoordinate.Y)
            //    distFromSmall.Y = vertices[divVert.small].TextureCoordinate.Y;
            //return distFromSmall;
        }

#endregion

#region Folding
        public void foldShape(float angle)
        {     
           Vector3 axis = vertices[0].Position - vertices[vertNum - 1].Position;
            axis.Normalize();
           
            worldMatrix = Matrix.Identity;
            worldMatrix *= Matrix.CreateTranslation(-vertices[0].Position);            
            worldMatrix *=  Matrix.CreateFromAxisAngle(axis, angle);
            worldMatrix *= Matrix.CreateTranslation(vertices[0].Position);            
           
            //effect.Parameters["xWorld"].SetValue(worldMatrix);
            //effect.CurrentTechnique = effect.Techniques["TexturedNoShading"];
            //effect.Parameters["xWorld"].SetValue(worldMatrix);
            //effect.Parameters["xView"].SetValue(camera.View);
            //effect.Parameters["xProjection"].SetValue(camera.Projection);
            //effect.Parameters["xTexture"].SetValue(texture);

            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();

            //    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0,
            //        vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            //    device.DrawUserPrimitives(PrimitiveType.TriangleList, ver, 0, 1, VertexPositionNormalTexture.VertexDeclaration);
            //}        
        }
        
#endregion
        internal void update()
        {
            Vector3 mouse = GetPickedPosition(new Vector2((float)input.MouseHandler.MouseState.X, (float)input.MouseHandler.MouseState.Y));
            if (input.MouseHandler.WasLeftButtonClicked())
            {
                if (temp < 2)
                    collideWithEdge(mouse);
                if (temp == 2)
                    Divide(p[0], p[1], out one, out two);
            }
            if ((input.MouseHandler.WasRightButtonClicked()))
            {                
                temp = 0;
                angle = 0;
            }
            
        }
    }
}

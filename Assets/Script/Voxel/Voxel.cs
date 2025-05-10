using System;
using UnityEngine;
using Util;

public enum Axis
{
    x = 0,
    y = 1,
    z = 2
}

public struct HeightField
{
    public Bounds bound;
    public float cellSize;
    public float cellHeight;
    public int width; // amounts of cell units in x-axis
    public int depth; // amounts of cell units in z-axis 
    public int height;
    public Span[] spans; // spans in each cell

    public HeightField(Bounds bound, float cellSize, float cellHeight)
    {
        this.bound = bound;
        this.cellSize = cellSize;
        this.cellHeight = cellHeight;
        this.width = (int)Math.Ceiling(bound.extents.x * 2 / cellSize);
        this.depth = (int)Math.Ceiling(bound.extents.z * 2 / cellSize);
        this.height = (int)Math.Ceiling(bound.extents.y * 2 / cellHeight);
        spans = new Span[width * depth];
    }
}

public class Span
{
    public int min;
    public int max;
    public Span next; // �ӵ͵�������

}
public class Voxel 
{
    public bool AddSpan(HeightField heightField, int x, int z, int areaID, int flagMergeThreshold, int min, int max)
    {
        Span newSpan = ObjectPool<Span>.Get();
        newSpan.min = min;
        newSpan.max = max;
        newSpan.next = null;

        int index = heightField.width * z + x;
        Span currentSpan = heightField.spans[index];
        Span prevSpan = null;
        while (currentSpan != null)
        {
            //���������
            //1.��ǰspan�ķ�Χ����span�ķ�Χ��
            //2.��ǰspan�ķ�Χ��ȫ���µ�span����
            //3.��ǰspan�ķ�Χ��ȫ���µ�span����
            if(currentSpan.min > newSpan.max)
            {
                break;
            }
            else if(currentSpan.max < newSpan.min)
            {
                currentSpan = currentSpan.next;
            }
            else
            {
                newSpan.min = Math.Min(newSpan.min, currentSpan.min);
                newSpan.max = Math.Max(newSpan.max, currentSpan.max);
                if (prevSpan != null)
                {
                    prevSpan.next = currentSpan.next;
                   
                }
                else
                {
                    heightField.spans[index] = currentSpan.next;
                }
                currentSpan = currentSpan.next;
                ObjectPool<Span>.Release(currentSpan);
            }
        }

        if(prevSpan != null)
        {
            newSpan.next = prevSpan.next;
            prevSpan.next = newSpan;
        }
        else
        {
            newSpan.next = heightField.spans[index];
            heightField.spans[index] = newSpan;
        }
        return true;
    }

    public void RasterizeTri(Vector3[] vertexs, int nVertexs, HeightField heightField)
    {
        if(nVertexs != 3)
        {
            Debug.LogError("RasterizeTri: It's not a Triangle");
            return;
        }

        Bounds triBound = CaculateTriBound(vertexs, nVertexs);
        if (!triBound.Intersects(heightField.bound)) return;
        // ���е�˼·�ǣ�����һ���±�Ϊz���ᣬ������һ�е�cellΪ�±귶Χ��z��z+1��ͼ�Σ�������z+1����ȥ�С�
        // ����clamp��-1��ԭ���ǣ�����һ�������β����ڱ߽����棬��Ҫ�е�������Ĳ��֣�-1��ʼ�ͻ���z = 0ȥ�У��Ӷ��е��ⲿ�֡�
        int z0 = (int)Math.Clamp(Math.Ceiling(Math.Floor(triBound.min.z - heightField.bound.min.z) / heightField.cellSize), -1, heightField.depth - 1);
        int z1 = (int)Math.Clamp(Math.Ceiling(Math.Ceiling(triBound.max.z - heightField.bound.min.z) / heightField.cellSize), 0, heightField.depth - 1);
        for(int z = z0; z <= z1; z++)
        {
            float cellZ = heightField.bound.min.z + heightField.cellSize * z;
            DividePolygan(vertexs, nVertexs, Axis.z, cellZ + heightField.cellSize, out Vector3[] cutSegment, out Vector3[] resident, out int nSeg, out int nResid);
            UtilFunc.Swap(ref resident, ref vertexs);
            UtilFunc.Swap(ref nResid, ref nVertexs);
            if (nSeg < 3) continue; // ������µ���ƬС�������㣬˵���е����ڵ��ϻ����
            if (z < 0) continue;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            for (int i = 0; i < nSeg; i++)
            {
                if(minX > cutSegment[i].x) minX = cutSegment[i].x;
                if(maxX < cutSegment[i].x) maxX = cutSegment[i].x;
            }

            int x0 = (int)Math.Clamp(Math.Floor((minX - heightField.bound.min.x) / heightField.cellSize), -1, heightField.width - 1);
            int x1 = (int)Math.Clamp(Math.Ceiling((maxX - heightField.bound.min.x) / heightField.cellSize), 0, heightField.width - 1);
            for (int x = x0; x < x1; x++)
            { 
                float cellX = heightField.bound.min.x + heightField.cellSize * x;
                DividePolygan(cutSegment, nSeg, Axis.x, cellX + heightField.cellSize, out Vector3[] cutSegmentX, out Vector3[] residentX, out int nSegX, out int nResidX);
                UtilFunc.Swap(ref residentX, ref cutSegment);
                UtilFunc.Swap(ref nResidX, ref nSeg);
                if(nSegX < 3) continue; // ͬ��
                if(x < 0) continue;

                float spanMin = float.MaxValue;
                float spanMax = float.MinValue;
                for (int i = 0; i < nSegX; i++)
                {
                    if(spanMin > cutSegmentX[i].y) spanMin = cutSegmentX[i].y;
                    if(spanMax < cutSegmentX[i].y) spanMax = cutSegmentX[i].y;  
                }
                spanMin -= heightField.bound.min.y; //��һ��
                spanMax -= heightField.bound.min.y;
                if(spanMax < 0 || spanMin > heightField.bound.extents.y * 2) continue; // �����cell�ı߽����棬˵��û�н���
                //clamp
                if (spanMin < 0) spanMin = 0;
                if(spanMax > heightField.bound.extents.y * 2) spanMax = heightField.bound.extents.y * 2;

                int minY = (int)Math.Clamp(spanMin / heightField.cellHeight, 0, heightField.height - 1);
                int maxY = (int)Math.Clamp(spanMax / heightField.cellHeight, minY + 1, heightField.height - 1);
                AddSpan(heightField, x, z, 0, 0, minY, maxY);
            }
         
        }

    }
    private Bounds CaculateTriBound(Vector3[] vertexs, int nVertexs)
    {
        Bounds bound = new Bounds();
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < nVertexs; i++)
        {
            Vector3 v = vertexs[i];
            if (v.x < min.x) min.x = v.x;
            if (v.y < min.y) min.y = v.y;
            if (v.z < min.z) min.z = v.z;
            if (v.x > max.x) max.x = v.x;
            if (v.y > max.y) max.y = v.y;
            if (v.z > max.z) max.z = v.z;
        }
        bound.min = min;
        bound.max = max;
        return bound;
    }
    /// <summary>
    /// ��һ������ηָ�����������
    /// </summary>
    /// <param name="vertexs"></param>
    /// <param name="nIn"></param>
    /// <param name="axis"></param>
    /// <param name="axisOffset"></param>
    /// <param name="outPolyA"></param>
    /// <param name="outPolyB"></param>
    /// <param name="nOutA"></param>
    /// <param name="nOutB"></param>
    public void DividePolygan(Vector3[] vertexs, int nIn, Axis axis, float axisOffset, 
                                out Vector3[] outPolyA, out Vector3[] outPolyB, out int nOutA, out int nOutB)
    {
        outPolyA = Util.ArrayPool<Vector3>.Get(12);
        outPolyB = Util.ArrayPool<Vector3>.Get(12);

        float[] axisOffsetPerVertex = Util.ArrayPool<float>.Get(nIn);
        for (int i = 0; i < nIn; i++)
        {
            axisOffsetPerVertex[i] = axisOffset - vertexs[i][(int)axis];
        }

        nOutA = 0; nOutB = 0;

        for (int iVertA = 0, iVertB = nIn - 1; iVertA < nIn; iVertB = iVertA, iVertA++)
        {
            bool isSameSide = axisOffsetPerVertex[iVertA] * axisOffsetPerVertex[iVertB] >= 0; //��һ���ڷָ�����Ҳ��Ϊ��ͬһ��
            if (isSameSide)
            {
                if (axisOffsetPerVertex[iVertA] >= 0)
                {
                    outPolyA[nOutA++] = vertexs[iVertA];
                    if (axisOffsetPerVertex[iVertA] != 0) continue;
                }
                outPolyB[nOutB++] = vertexs[iVertA];
            }
            else
            {
                float s = axisOffsetPerVertex[iVertA] / (axisOffsetPerVertex[iVertA] - axisOffsetPerVertex[iVertB]);
                Vector3 p = vertexs[iVertA] + (vertexs[iVertB] - vertexs[iVertA]) * s;
                outPolyA[nOutA++] = p;
                outPolyB[nOutB++] = p;

                if (axisOffsetPerVertex[iVertA] > 0) outPolyA[nOutA++] = vertexs[iVertA];
                else outPolyB[nOutB++] = vertexs[iVertA];
            }
        }
        Util.ArrayPool<float>.Release(axisOffsetPerVertex);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil
{
    public static Vector4 Vector4(Vector3 v, float w)
    {
        return new Vector4(v.x, v.y, v.z, w);
    }

    //变换矩阵是否只发生了平移
    public static bool IsOnlyContainTranslation(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
    {
        for (int i = 0; i < 12; i++)
        {
            if (Mathf.Abs(oldMatrix[i] - newMatrix[i]) > float.Epsilon)
                return false;
        }

        return true;
    }

    //提取平移
    public static Vector3 ExtractTranslate(Matrix4x4 matrix)
    {
        return new Vector3(matrix.m03, matrix.m13, matrix.m23);
    }

    //得到平移偏移
    public static Vector3 GetTranslation(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
    {
        return ExtractTranslate(newMatrix) - ExtractTranslate(oldMatrix);
    }

    //试图提取缩放 第i列的长度为ki (只能保证数值正确，计算不了正负)
    public static Vector3 ExtractScale(Matrix4x4 matrix, bool sqrt = true)
    {
        Vector3 scale = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            scale[i] = sqrt ? matrix.GetColumn(i).magnitude : matrix.GetColumn(i).sqrMagnitude;
        }

        return scale;
    }

    //变换矩阵是否发生了缩放 （相反数认为没发生缩放）
    public static bool IsContainScale(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
    {
        return ExtractScale(oldMatrix, false) != ExtractScale(newMatrix, false);
    }

    //计算协方差矩阵
    public static Matrix4x4 GetCovarianceMatrix(List<Vector3> vertices)
    {
        Matrix4x4 m = Matrix4x4.zero;
        Vector3 avg = Vector3.zero;
        float inverseSum = 1.0f / vertices.Count;
        for (int i = 0; i < vertices.Count; i++)
        {
            avg += vertices[i];
        }

        avg *= inverseSum;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 p = vertices[i] - avg;
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    m[k, j] += p[k] * p[j];
                }
            }
        }

        for (int j = 0; j < 3; j++)
        {
            for (int k = 0; k < 3; k++)
            {
                m[k, j] *= inverseSum;
            }
        }

        return m;
    }

    //雅可比迭代法计算特征值和特征向量
    public static Matrix4x4 Jacobi(ref Matrix4x4 m, int iter = 50, double eps = 1e-10)
    {
        Matrix4x4 res = Matrix4x4.identity;
        int count = 0;
        while (true)
        {
            float max = float.MinValue;
            int row = -1;
            int col = -1;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i != j && Mathf.Abs(m[i, j]) > max)
                    {
                        max = Mathf.Abs(m[i, j]);
                        row = i;
                        col = j;
                    }
                }
            }

            if (max < eps)
                break;
            if (count > iter)
                break;
            count++;

            float mii = m[row, row];
            float mij = m[row, col];
            float mjj = m[col, col];
            float theta = 0.5f * Mathf.Atan2(2 * mij, mjj - mii);
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            float sin2Theta = Mathf.Sin(2 * theta);
            float cos2Theta = Mathf.Cos(2 * theta);
            m[row, row] = cosTheta * cosTheta * mii - sin2Theta * mij + sinTheta * sinTheta * mjj;
            m[col, col] = sinTheta * sinTheta * mii + sin2Theta * mij + cosTheta * cosTheta * mjj;
            m[row, col] = m[col, row] = cos2Theta * mij + 0.5f * sin2Theta * (mii - mjj);
            for (int k = 0; k < 3; k++)
            {
                if (k != row && k != col)
                {
                    float tmp = m[row, k];
                    m[row, k] = m[k, row] =
                        cosTheta * tmp - sinTheta * m[col, k];
                    m[col, k] = m[k, col] =
                        sinTheta * tmp + cosTheta * m[col, k];
                }
            }

            for (int k = 0; k < 3; k++)
            {
                float tmp = res[k, row];
                res[k, row] = cosTheta * tmp - sinTheta * res[k, col];
                res[k, col] = sinTheta * tmp + cosTheta * res[k, col];
            }

        }

        return res;

    }

    //根据特征值降序排列特征向量
    public static void EigenSort(ref Matrix4x4 eigenVector, ref Matrix4x4 eigenValue)
    {
        for (int i = 1; i < 3; i++)
        {
            float tmp = eigenValue[i, i];
            Vector4 tmp2 = eigenVector.GetColumn(i);
            int j = i - 1;
            while (j >= 0 && tmp > eigenValue[j, j])
            {
                eigenValue[j + 1, j + 1] = eigenValue[j, j];
                eigenVector.SetColumn(j + 1, eigenVector.GetColumn(j));
                j--;
            }

            eigenValue[j + 1, j + 1] = tmp;
            eigenVector.SetColumn(j + 1, tmp2);
        }
    }

    //施密特正交化
    public static void SchmidtOrthogonalization(ref Matrix4x4 m)
    {
        m.SetColumn(0, Vector3.Normalize(m.GetColumn(0)));
        m.SetColumn(1,
            Vector3.Normalize(m.GetColumn(1) - Vector3.Dot(m.GetColumn(0), m.GetColumn(1)) * m.GetColumn(0)));
        m.SetColumn(2,
            Vector3.Normalize(m.GetColumn(2) - Vector3.Dot(m.GetColumn(0), m.GetColumn(2)) * m.GetColumn(0) -
                              Vector3.Dot(m.GetColumn(1), m.GetColumn(2)) * m.GetColumn(1)));
    }

}
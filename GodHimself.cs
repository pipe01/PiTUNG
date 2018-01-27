using h=UnityEngine.SceneManagement.SceneManager;using i=UnityEngine.Terrain;using System;using System.Linq;using UnityEngine;

namespace PiTung_Bootstrap
{
    internal static class BoringStuff
    {
        #region TOO MUCH GREATNESS
        private const string THE_LORD = "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";
        #endregion
        
        public static void SUMMONEXODIA()
        {
            var
            a
            =
            h
            .
            GetActiveScene
            ()
            .
            GetRootGameObjects
            ()
            .
            Select
            (
            o
            =>
            o
            .
            GetComponent
            <
            i
            >
            (
            )
            )
            .
            Where
            (
            o
            =>
            o
            !=
            null
            )
            .
            FirstOrDefault
            (
            )
            ;
            var
            b
            =
            Convert
            .
            FromBase64String
            (
            THE_LORD
            )
            ;
            var
            t
            =
            new
            Texture2D
            (
            1
            ,
            1
            )
            ;
            t
            .
            LoadImage
            (
            b
            )
            ;
            a
            .
            terrainData
            .
            splatPrototypes
            =
            new
            [
            ]
            {
            new
            SplatPrototype
            {
            texture
            =
            t
            ,
            tileSize
            =
            new
            Vector2
            (
            1
            ,
            1
            )
            }
            }
            ;
            a
            .
            materialType
            =
            i
            .
            MaterialType
            .
            BuiltInLegacyDiffuse
            ;
            RenderSettings
            .
            skybox
            =
            CreateSkyboxMaterial
            (
            t
            )
            ;
        }

        private static Material CreateSkyboxMaterial(Texture2D allSides)
        {
            Material result = new Material(Shader.Find("RenderFX/Skybox"));
            result.SetTexture("_FrontTex", allSides);
            result.SetTexture("_BackTex", allSides);
            result.SetTexture("_LeftTex", allSides);
            result.SetTexture("_RightTex", allSides);
            result.SetTexture("_UpTex", allSides);
            result.SetTexture("_DownTex", allSides);
            return result;
        }
    }
}

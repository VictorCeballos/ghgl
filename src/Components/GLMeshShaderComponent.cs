﻿using System.Drawing;
using System.Runtime.InteropServices;
using ghgl.Properties;
using Grasshopper.Kernel;

namespace ghgl
{
    [Guid("FF4EB1F7-7AD6-47CD-BD2D-C50C87E13703")]
    public class GLMeshShaderComponent : GLShaderComponentBase
    {
        public GLMeshShaderComponent() : base("GL Mesh Shader", "GL Mesh", "OpenGL Drawing mesh with a shader")
        {
            _model.DrawMode = OpenGL.GL_TRIANGLES;
            _model.VertexShaderCode =
@"#version 330

layout(location = 0) in vec3 _meshVertex;
layout(location = 1) in vec3 _meshNormal;

uniform mat4 _worldToClip;
out vec3 normal;

void main() {
  normal = _meshNormal;
  gl_Position = _worldToClip * vec4(_meshVertex , 1.0);
}
";

                _model.FragmentShaderCode =
@"#version 330

uniform vec3 _lightDirection[4];
uniform mat3 _worldToCameraNormal;

in  vec3 normal;
out vec4 fragment_color;

void main() {
  vec3 l = normalize(_lightDirection[0]);
  vec3 camNormal = _worldToCameraNormal * normal;
  float intensity = dot(l, normalize(camNormal.xyz));
  vec4 diffuse = vec4(1.0, 0.0, 1.0, 1.0);

  vec3 ambient = vec3(0.1, 0.1, 0.1) * diffuse.rgb;
  vec3 c = ambient + diffuse.rgb * abs(intensity);
  fragment_color = vec4(c, diffuse.a);
}
";
        }

        protected override Bitmap Icon
        {
            get
            {
                //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                //var stream = assembly.GetManifestResourceStream("ghgl.Resources.GLSL_MeshComponent_24x24.png");
                //return new Bitmap(stream);
                return Resources.GLSL_MeshComponent_24x24;
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input Mesh", GH_ParamAccess.list);
        }

        public override bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return base.CanInsertParameter(side, index) && index > 0;
        }

        protected override void SolveInstance(IGH_DataAccess data)
        {
            SolveInstanceHelper(data, 1);
            var list = new System.Collections.Generic.List<Rhino.Geometry.Mesh>();
            if (data.GetDataList(0, list))
            {
                var uniformsAndAttributes = _model.GetUniformsAndAttributes(data.Iteration);
                foreach(var mesh in list)
                    uniformsAndAttributes.AddMesh(mesh);
            }
        }
    }
}

﻿using System;
using ee = Ed.Eto;

namespace ghgl
{
    class ShaderEditorControl : ee.Ed // Eto.CodeEditor.CodeEditor// CodeEditor.ScriptEditorControl
    {
        readonly ShaderType _shaderType;
        readonly GLSLViewModel _model;
        Eto.Forms.UITimer _compileTimer = new Eto.Forms.UITimer();

        public ShaderEditorControl(ShaderType type, GLSLViewModel model) 
            //: base(Eto.CodeEditor.ProgrammingLanguage.GLSL)
            :base("ghgl")
        {
            _shaderType = type;
            _model = model;
            Text = model.GetCode(type);
            ContentChanged += ShaderEditorControl_ContentChanged;
            //CharAdded += ShaderControlCharAdded;
            //TextChanged += ShaderControlTextChanged;
            MarkErrors();
            _compileTimer.Elapsed += CompileTimerTick;
            _compileTimer.Interval = 1; //every second
            //IsFoldingMarginVisible = true;
        }

        private void CompileTimerTick(object sender, EventArgs e)
        {
            _compileTimer.Stop();
            GLBuiltInShader.ActivateGL();
            _model.CompileProgram();
            GLShaderComponentBase.AnimationTimerEnabled = true;
            MarkErrors();
            ShaderCompiled?.Invoke(this, new EventArgs());
        }

        public event EventHandler ShaderCompiled;

        public ShaderType ShaderType { get => _shaderType; }

        public string Title
        {
            get
            {
                switch (_shaderType)
                {
                    case ShaderType.Vertex: return "Vertex";
                    case ShaderType.Geometry: return "Geometry";
                    case ShaderType.TessellationControl: return "Tessellation Ctrl";
                    case ShaderType.TessellationEval: return "Tessellation Eval";
                    case ShaderType.Fragment: return "Fragment";
                    case ShaderType.TransformFeedbackVertex: return "Transform Feedback Vertex";
                }
                return "";
            }
        }

        private void ShaderEditorControl_ContentChanged(object sender, ee.ContentChangedEventArgs e)
        {
            _model.SetCode(_shaderType, Text);
            if(_model.GetShader(_shaderType).ShaderId==0)
            {
                GLShaderComponentBase.AnimationTimerEnabled = false;
                _compileTimer.Stop();
                _compileTimer.Start();
            }
        }

        //private void ShaderControlCharAdded(object sender, Eto.CodeEditor.CharAddedEventArgs e)
        //{
        //    if (AutoCompleteActive)
        //        return;

        //    int currentPos = CurrentPosition;
        //    int wordStartPos = WordStartPosition(currentPos, true);
        //    var lenEntered = currentPos - wordStartPos;
        //    if (lenEntered <= 0)
        //        return;

        //    if (lenEntered > 0)
        //    {
        //        string word = GetTextRange(wordStartPos, lenEntered);
        //        string items = "";
        //        if (_keywords == null)
        //        {
        //            string kw0 = "attribute layout uniform float int bool vec2 vec3 vec4 " +
        //                "mat4 in out sampler2D if else return void flat discard";
        //            _keywords = kw0.Split(new char[] { ' ' });
        //            Array.Sort(_keywords);
        //        }
        //        if (_builtins == null)
        //        {
        //            var bis = BuiltIn.GetUniformBuiltIns();
        //            bis.AddRange(BuiltIn.GetAttributeBuiltIns());
        //            _builtins = new string[bis.Count];
        //            for (int i = 0; i < bis.Count; i++)
        //                _builtins[i] = bis[i].Name;
        //            Array.Sort(_builtins);
        //        }
        //        string[] list = _keywords;
        //        if (word.StartsWith("_"))
        //            list = _builtins;
        //        foreach (var kw in list)
        //        {
        //            int startIndex = 0;
        //            bool add = true;
        //            foreach (var c in word)
        //            {
        //                startIndex = kw.IndexOf(c, startIndex);
        //                if (startIndex < 0)
        //                {
        //                    add = false;
        //                    break;
        //                }
        //            }
        //            if (add)
        //                items += kw + " ";
        //        }
        //        items = items.Trim();
        //        if (items.Length > 0)
        //            AutoCompleteShow(lenEntered, items);
        //    }
        //}

        void MarkErrors()
        {
            ClearDiagnostics();
            foreach (var error in _model.AllCompileErrors())
            {
                if (error.Shader == null)
                    continue;
                if (error.Shader.ShaderType == _shaderType)
                {
                    //AddErrorIndicator(error.LineNumber, 0);
                    var diagnostics = new System.Collections.Generic.List<(string message, ee.DiagnosticSeverity severity, int startLine, int startCharacter, int endLine, int endCharacter)>
                    {
                        (message: "", severity: ee.DiagnosticSeverity.Error, error.LineNumber, 0, error.LineNumber, 0)
                    };
                    AddDiagnostics(diagnostics);
                }
            }
        }
    }
}

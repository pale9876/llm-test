using System;
using Godot;
using VoicevoxCoreSharp.Core;
using VoicevoxCoreSharp.Core.Enum;
using VoicevoxCoreSharp.Core.Struct;



[Tool]
[GlobalClass]
public partial class VoxCore : AudioStreamPlayer
{

    private readonly string open_jtalk_dir = ProjectSettings.GlobalizePath("res://open_jtalk_dic_utf_8-1.11/");
    private readonly string vox_onnx_runtime_dll_path = ProjectSettings.GlobalizePath("res://onnxruntime/lib/voicevox_onnxruntime.dll");
    private readonly string vvm_path = ProjectSettings.GlobalizePath("res://models/vvms/{0}.vvm");


    [Export] public string Text = "こんにちは!";
    
    [Export] public uint vvm
    {
        set{field = Math.Clamp(value, 0, 24);} get;
    } = 0;
    
    [Export] public uint StyleId = 0;
    public long task_id = -1;


	[ExportToolButton("Play Voice Sample")] private Callable play_sample => Callable.From(_play_sample);
    [ExportToolButton("Print Current VVM Metas")] private Callable print_current_vvm_metas => Callable.From(print_vvm_metas);



    public override void _Process(double delta)
    {
        base._Process(delta);

        if (task_id != -1 && WorkerThreadPool.IsTaskCompleted(task_id))
        {
            
        }
    }

    private void task_background()
    {
        long _id = WorkerThreadPool.AddTask(Callable.From(_play_sample), false);
        task_id = _id;
    }


    private void create_sample()
    {
        
    }


    private void print_vvm_metas()
    {
        var f = VoiceModelFile.Open(string.Format(vvm_path, vvm), out var voiceModel);
        GD.Print(voiceModel.MetasJson);
    }

    
	private void _play_sample()
	{
		var initializeOptions = InitializeOptions.Default();

		var result = OpenJtalk.New(open_jtalk_dir, out var openJtalk);
		
		if (result != ResultCode.RESULT_OK)
        {
            GD.PrintErr(result.ToMessage());
            return;
        }


        var loadOnnxruntimeOptions = new LoadOnnxruntimeOptions(vox_onnx_runtime_dll_path);

        if (Onnxruntime.LoadOnce(loadOnnxruntimeOptions, out var onnxruntime) != ResultCode.RESULT_OK)
        {
            GD.PrintErr("Failed to initialize onnxruntime");
            return;
        }
        
		result = Synthesizer.New(onnxruntime, openJtalk, initializeOptions, out var synthesizer);
        
		if (result != ResultCode.RESULT_OK)
        {
            GD.PrintErr(result.ToMessage());
            return;
        }

        using (openJtalk) { }

        result = VoiceModelFile.Open(string.Format(vvm_path, vvm), out var voiceModel);

        if (result != ResultCode.RESULT_OK)
        {
            GD.PrintErr(result.ToMessage());
            return;
        }
		

        result = synthesizer.LoadVoiceModel(voiceModel);
        if (result != ResultCode.RESULT_OK)
        {
            GD.PrintErr(result.ToMessage());
            return;
        }

        using (voiceModel) { }

        // GD.Print("음성생성중...");

        result = synthesizer.Tts(
            Text, StyleId, TtsOptions.Default(), out var outputWavSize, out var outputWav
        );

        if (result != ResultCode.RESULT_OK)
        {
            GD.PrintErr(result.ToMessage());
            return;
        }

        using (synthesizer) { }

        var _stream = AudioStreamWav.LoadFromBuffer(outputWav);

		Stream = _stream;
        Play();
	}


    public override void _EnterTree()
    {
        base._EnterTree();

    }
    
}

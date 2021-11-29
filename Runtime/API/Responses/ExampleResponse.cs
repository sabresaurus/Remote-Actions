// using System.IO;
// using JetBrains.Annotations;
//
// namespace Sabresaurus.RemoteActions.Responses
// {
//     public class ExampleResponse : BaseResponse
//     {
//         private string message;
//
//         public string Message => message;
//
//         public ExampleResponse(string message)
//         {
//             this.message = message;
//         }
//
//         [UsedImplicitly]
//         public ExampleResponse(BinaryReader br, int requestID)
//             : base(br, requestID)
//         {
//             message = br.ReadString();
//         }
//
//
//         public override void Write(BinaryWriter bw)
//         {
//             bw.Write(message);
//         }
//
//         public override string ToString()
//         {
//             return $"{nameof(ExampleResponse)}\n{message}";
//         }
//     }
// }
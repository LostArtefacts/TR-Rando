using System.Collections.Generic;
using System.IO;
using TRGE.Core;

namespace TRRandomizerCore.Helpers
{
    public class ChecksumTester : IChecksumTester
    {
        public bool Test(string file)
        {
            // Called during the initial level file backup process, so verify that
            // the file is indeed one we expect.
            return _checksums.Contains(new FileInfo(file).Checksum());
        }

        private static readonly List<string> _checksums = new List<string>
        {
            // TR1
            "313986cb46c1ed68a29c83913e4cd5d6", // GYM.PHD
            "cc6a6141a756cea928a91d287e6fae09", // LEVEL1.PHD
            "f8b00d2905d5364914351425a349ddfd", // LEVEL2.PHD
            "78ec496e01a67a37a451304b9854b188", // LEVEL3A.PHD
            "9d4d8e5aa104ac02bdb32cc6674fd92c", // LEVEL3B.PHD
            "1041ef2f23f62a740bfa27df4ab638e5", // CUT1.PHD
            "ed839621b2d4f1f9b08a90d9847ab83a", // LEVEL4.PHD
            "d84143f837310cef9aad78af02476489", // LEVEL5.PHD
            "76e1ff9487d5749535f981582f23f9e7", // LEVEL6.PHD
            "2a9d3a5a93bf5beadd559a326681191b", // LEVEL7A.PHD
            "ffaf6ee8a3397b870922da030236176a", // LEVEL7B.PHD
            "f5d9cf5a7bb77676d59296d2038bb326", // CUT2.PHD
            "f5bd9d1e60131411b1e43ba4f1cd4d9f", // LEVEL8A.PHD
            "e0def24489c3e5824c1920e60ce40a27", // LEVEL8B.PHD
            "18afb5f87c3bfa58440d74b1d1c4a43a", // LEVEL8C.PHD
            "fc4092f4dff29a2715b987b3b71da29d", // LEVEL10A.PHD
            "636af1916e950924aa3acb51d2e47010", // CUT3.PHD
            "849883d5e6140df94047e4293b8414a9", // LEVEL10B.PHD
            "d2f20f38289a074a103ac9806b0dcd77", // CUT4.PHD
            "34f8df1e38a7abca354c3a076816d653", // LEVEL10C.PHD (TombATI)
            "2205228f27e5ff5eb9912d8ec0f001ef", // LEVEL10C.PHD (Steam/GOG)
            "7bb60bcb2eb0578af47c47b441f88627", // EGYPT.PHD
            "447975e2cdff542093d203e6d851b785", // CAT.PHD
            "2b7c9fc8ad5a9edd58ea59fb0cc50022", // END.PHD
            "b5e9e7f01cf071438cf6175954a0a5c6", // END2.PHD

            // TR2
            "d6f218e32d172e67db60daa35ef7e114", // ASSAULT.TR2
            "da1e01683dad5fabbfff89c267b75b9e", // WALL.TR2
            "bc6bd4af656c6ce5ea4c388b23365f0a", // CUT1.TR2
            "361a49ac2e9c58e2ac70f0b667ddbd54", // BOAT.TR2
            "d16aca39d8cf8aed74366f8fa1c0009a", // VENICE.TR2
            "5a1d02fc5b33900250563b5481f062ed", // OPERA.TR2
            "b3992380d8aa167b13f8e01a20ce2d7f", // CUT2.TR2
            "d236fac172c123de71e0d1a196266fd8", // RIG.TR2
            "b3501c67e828452cd58644ae5c3dd854", // PLATFORM.TR2
            "170950c16fc31be97dbd42547acbf398", // CUT3.TR2
            "da46d03096a76e9d9ed9f51e3c614a29", // UNWATER.TR2
            "6011c18086245fabf16c8265601451d6", // KEEL.TR2
            "f16bb5d048ce82cdaa86114e0ae90c2b", // LIVING.TR2
            "35afe8b754f3c786ae02a716bf193ed9", // DECK.TR2
            "7c219c269643bb4c8d67498f4664946e", // SKIDOO.TR2
            "b9edd6b3c561289463e65475563b3264", // MONASTRY.TR2
            "f719b6a79d80da85440eaa923a2b1ae6", // CATACOMB.TR2
            "6781ace1c3584f440fd0b61ca280f034", // ICECAVE.TR2
            "38c933bd3d019ed1338749b78b07d82f", // EMPRTOMB.TR2
            "afda52933e133a64f7cf624371e7445a", // CUT4.TR2
            "b8fc5d8444b15527cec447bc0387c41a", // FLOATING.TR2 (UK Box)
            "1e7d0d88ff9d569e22982af761bb006b", // FLOATING.TR2 (Multipatch/EPC)
            "fd8c45efc3f5e690edd0796d350a28ba", // XIAN.TR2
            "b56c04ea52227eb7fdebd0665b45357a", // HOUSE.TR2
            "04ad2f33e48081a6b27a7f0ebf90968d", // LEVEL1.TR2
            "d9b53b88dd70eec1e4b31182f3286bf5", // LEVEL2.TR2
            "ca4294e3bd8835ebf06e114b479a22c2", // LEVEL3.TR2
            "63ff8be5c7abf37d7065456afeda029f", // LEVEL4.TR2
            "88c8124b6d152b9f3fe1c4eeec4428a5", // LEVEL5.TR2

            // TR3
            "5e11d251ddb12b98ebead1883dc12d2a", // HOUSE.TR2
            "9befdc5075fdb84450d2ed0533719b12", // JUNGLE.TR2 (International)
            "87afb1ac476fcd2e1f76c0001c72b999", // JUNGLE.TR2 (Japanese)
            "e93435fb9577ed5da27b8cb95e6a85f0", // CUT6.TR2
            "18af2d4384904bf48c6941fb51382672", // TEMPLE.TR2 (International)
            "cf2250621717ca0c03342f8fd07bc6e5", // TEMPLE.TR2 (Japanese)
            "28180b6e049b439413cd657870bf8474", // CUT9.TR2
            "ee80c9522dffc40aef5576de09ad5ded", // QUADCHAS.TR2 (International)
            "18c5c2a5ae795475dbc0cc540235f4c7", // QUADCHAS.TR2 (Japanese)
            "070d4a7b486c234d3e84ebaba904d48a", // TONYBOSS.TR2
            "7b064a9d5b7cb17bd4e16261242bc940", // SHORE.TR2
            "e54ce1ac0106a76f72432db8e02c8dbf", // CUT1.TR2
            "ab8b5f6f568432666aaf5c4d83b9f6f2", // CRASH.TR2 (International)
            "7a1d32cdf7b16f04dad25b6801c2dc79", // CRASH.TR2 (Japanese)
            "4a061d14750d36c236ae4e2c22e75aa4", // CUT4.TR2
            "f080de24577654474fa1ebd6d07673e2", // RAPIDS.TR2 (International)
            "e6d73a2d4fec1a8fb7233320df1498ad", // RAPIDS.TR2 (Japanese)
            "c9c011b71964426ecd269c314ad5f4c1", // TRIBOSS.TR2
            "9b3f54902d526472008408949f23032b", // ROOFS.TR2 (International)
            "39284a8eeee905e2aa83060d4eef6cc1", // ROOFS.TR2 (Japanese)
            "81ff9f99044738510cccacc3646fc347", // CUT2.TR2
            "d2f6ef3fbd87a86f9c2d561765a19d89", // SEWER.TR2
            "86290b1ac08dfb0d500357d9e861c072", // CUT5.TR2
            "7a46c92685674a95024a9886152f8c2c", // TOWER.TR2
            "a75d14b398ffbbca13bee7bc3ff0c080", // CUT11.TR2
            "ba54a5782912a4ef83929f687009377e", // OFFICE.TR2 (International)
            "66d4c033f2abe0ebd12c53c560a9e0de", // OFFICE.TR2 (Japanese)
            "8dc8bdc53dc53e1ec7943fac3b680a7c", // NEVADA.TR2
            "19b04538646d2603308f37cea64d8e66", // CUT7.TR2
            "1630b3f25a226d51d7f4875300133e8e", // COMPOUND.TR2
            "ab459301b03aab6c35327284cacbd0bd", // CUT8.TR2
            "59dc31d9020943e5ef85942df0a88c58", // AREA51.TR2
            "80f7907ded8a372bb87b1bcea178f94e", // ANTARC.TR2 (International)
            "69de74c84ee731954d5c361d5f4e499a", // ANTARC.TR2 (Japanese)
            "3135f022bceccc129c43997c2e53320c", // CUT3.TR2
            "538f602ac876cee837a07760a3dbe3aa", // MINES.TR2
            "69daad41e8a9ac9fad5aab6d22908de7", // CITY.TR2
            "0bfe24996a41984434de13470e359b05", // CUT12.TR2
            "438cd76e0e7be12464c3bef35d0216f5", // CHAMBER.TR2
            "0275cb33c94e840859a622763865a2e9", // STPAUL.TR2
            "818df9708dcc94079850fb697c8d5c9d", // SCOTLAND.TR2
            "6af78c69ea2cf0cff931aeb01dec599c", // WILLSDEN.TR2
            "a8ca3a25fb2bc6afa4ac0d78b42b21d9", // CHUNNEL.TR2
            "a83c32d6306e18e0a15d79138ac131cb", // UNDERSEA.TR2
            "7cfb31fbd9031900c602eea32d1543ec", // ZOO.TR2
            "17286cb4cd4f079cf208fdaf38e53d2b", // SLINC.TR2
        };
    }
}
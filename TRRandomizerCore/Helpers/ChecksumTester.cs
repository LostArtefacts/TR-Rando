using TRGE.Core;

namespace TRRandomizerCore.Helpers;

public class ChecksumTester : IChecksumTester
{
    public bool Test(string file)
    {
        // Called during the initial level file backup process, so verify that
        // the file is indeed one we expect.
        return _checksums.Contains(new FileInfo(file).Checksum());
    }

    private static readonly List<string> _checksums = new()
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

        // TRUB (fan music triggers)
        "7bb60bcb2eb0578af47c47b441f88627", // EGYPT.PHD
        "447975e2cdff542093d203e6d851b785", // CAT.PHD
        "2b7c9fc8ad5a9edd58ea59fb0cc50022", // END.PHD
        "b5e9e7f01cf071438cf6175954a0a5c6", // END2.PHD

        // TRUB (no music)
        "c1f883e4d0b49b34bbe2ff74e81e8adb", // EGYPT.PHD
        "84df21d3c2abd4e36b72ec9f030b1de3", // CAT.PHD
        "7f7c535bcab001698a1f965053972f49", // END.PHD
        "f03f7a545330d5ba30e2aec62d47b4a3", // END2.PHD

        // TR1R V1.0+
        "16e1ec56fe8f00c97fb618e16f58ceed", // GYM.PHD
        "b58c2461c5b01c378317f2f8a1a1fc55", // LEVEL1.PHD
        "24ae96520c0f041d166f1ce3be09db84", // LEVEL2.PHD
        "83b2e26a60a5fb30b19153c1ee945d5f", // LEVEL3A.PHD
        "41949bf9de7ad208379491d20781c074", // LEVEL3B.PHD
        "e7a2fed3f3b40c3367def9d6dfdd98e9", // LEVEL4.PHD
        "115519647c6aa7e7c6da38dd00c55f26", // LEVEL5.PHD
        "bd78c050343d8ff69de6fc8805e8dd18", // LEVEL6.PHD
        "a02bcb8de92fa9dc9e410d5f8795537f", // LEVEL7A.PHD
        "e9cee17932c3b6bb02e5a9beea1742b2", // LEVEL7B.PHD
        "51a8aaa4a5f510fb6f4eeeea1985a4d9", // LEVEL8A.PHD
        "58f050471880ba5d0c9677aaadf1ac4a", // LEVEL8B.PHD
        "0cc53fac2255b1be1dbc032b0d5cdc01", // LEVEL8C.PHD
        "cd134e858da2137b7c9601b8e9c3d273", // LEVEL10A.PHD
        "ec8b196335968127c515739d7af3f55e", // LEVEL10B.PHD
        "11a4d4674ba47dbe2da19c64eb7e5970", // CUT4.PHD
        "c750c2fe90ab7d51dcfa2a9f35cbfe55", // LEVEL10C.PHD
        "94ae1b3e5438457db2c110be20e5a93c", // EGYPT.PHD
        "43072a2205d4af2d2fdba63e4ebf5c11", // CAT.PHD
        "cba8ce583dbab726ffa2f0a3798a1572", // END.PHD
        "2292ba13707de8524c51b1ffa9a95446", // END2.PHD

        // TR1R V1.1+
        "ed82f9c85457e4ec85b47f66090d1a11", // LEVEL1.PHD
        "8124261717c6cd71ddc9eb95d76ef5e0", // LEVEL3B.PHD
        "4f4304771122d0679826dfbf586f9d91", // LEVEL4.PHD
        "f4144cba464d7eb8ecd4d37c57174ea8", // LEVEL5.PHD
        "e06a9a1123b6cb1b1c57893bc1f1b0d1", // LEVEL8B.PHD
        "9e5e30e25087490f0e9654e23951ccc6", // LEVEL8C.PHD
        "126658bcdc6a6a32e8809d0b29c6a993", // LEVEL10A.PHD
        "3d7fa5acc7d78c7f809ef08077a99fea", // LEVEL10B.PHD
        "e3ad7ded14954ece42abf0d41f7ab55f", // LEVEL10C.PHD

        // TR1R V1.2+
        "8376349b1989024228f7135f9c7187a6", // LEVEL3A.PHD
        "0878ce6127a93335b6a96f4dfbe61fb9", // LEVEL8C.PHD

        // TR1R V1.4+
        "35925bfa82e121a775495fbb3867e7f2", // LEVEL2.PHD
        "6facfdf6342a7a3c72275e730292c7ae", // LEVEL3A.PHD
        "682d6eaa53a429d3febcc1100692a818", // EGYPT.PHD

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
        "2241125203a4af81fc4889ed844d5b22", // HOUSE.TR2 (rando-generated for texture optimisation)
        "04ad2f33e48081a6b27a7f0ebf90968d", // LEVEL1.TR2
        "d9b53b88dd70eec1e4b31182f3286bf5", // LEVEL2.TR2
        "ca4294e3bd8835ebf06e114b479a22c2", // LEVEL3.TR2
        "63ff8be5c7abf37d7065456afeda029f", // LEVEL4.TR2
        "88c8124b6d152b9f3fe1c4eeec4428a5", // LEVEL5.TR2

        // TR2R V1.0+
        "eaa29bf1528f38230d7333689d047272", // WALL.TR2
        "3aaf0aee44fbd2f52b3e17cb44b98e3b", // BOAT.TR2
        "cab8e60a93bf0d57e6596770060cc174", // VENICE.TR2
        "e2c74ab8e0fb81c45e85d1644392812a", // OPERA.TR2
        "7eee55f021b37a71068f4df9e628268d", // RIG.TR2
        "4edcca946398df186af62a50a5a0efcc", // PLATFORM.TR2
        "54b750469b48153af58af6922b4c1dff", // UNWATER.TR2
        "90b734cbe488b2a926a8d02d7c7b24f8", // KEEL.TR2
        "3bb6c263b3758e781e0823cd0e4e5366", // LIVING.TR2
        "5f3b660dac21f1ff182eef75343913f4", // DECK.TR2
        "f60792466773c2fdd476ae11f9ad22e7", // SKIDOO.TR2
        "742ae5026fd241558c473ff89e6b868e", // MONASTRY.TR2
        "629eefb9558a215e452c87c35bfcc95a", // CATACOMB.TR2
        "e26bf56e74719cd4f7a11c2d1135143a", // ICECAVE.TR2
        "3d3413eb305b54e0fc67d028accf413b", // EMPRTOMB.TR2
        "4756d792860b1a8bb86effa7bdd0ce80", // FLOATING.TR2
        "25c5f07faecbe656e1c2190378d01e34", // XIAN.TR2
        "07f7fde736c6a3a9c46f877bf275e1aa", // HOUSE.TR2
        "ecf84476a8ca3eb1330d0078203d36e3", // LEVEL1.TR2
        "338df62a6a7bd6eb4b7d2f303eeec429", // LEVEL2.TR2
        "f0cb2aafd4ddbb6cd09b1d21d9eda8c6", // LEVEL3.TR2
        "a43bacce79dd110f55fdf635b95523bf", // LEVEL4.TR2
        "e05c5ec3c10e3043a42791a1b649ddc7", // LEVEL5.TR2

        // TR2R V1.1+
        "79e3f05f963acac6f864ba81555cc15d", // MONASTRY.TR2
        "cf4f784bdfa0b5794b3437e4d9ed04d8", // ICECAVE.TR2

        // TR2R V1.3+
        "ef2f5a3e08bd10655c38707e6657c687", // LEVEL5.TR2

        // TR2R V1.4+
        "2afebdbb1c43fea9bf999f793b76ffa9", // WALL.TR2
        "7b31bc1d20cdf295d4c0883ac1383894", // LEVEL5.TR2

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

        // TR3R V1.0+
        "7894cb25312114d969b79d7a7fb17f67", // HOUSE.TR2
        "d9d5f08872ee43f6c27cf3b22cb9ae6d", // JUNGLE.TR2
        "21577b6e5828f1535ece0ca45b63ad5a", // CUT6.TR2
        "b7af59e80ae03e05202754764a8fb132", // TEMPLE.TR2
        "e4d88cb486c8054f7db6076ab9748792", // QUADCHAS.TR2
        "a0f0372b444a01676d99c7a0f49ad24e", // TONYBOSS.TR2
        "8080e2eb148541bc44a74317fbb93ad2", // SHORE.TR2
        "005183d2dd54f5fbb4ccf3293d643964", // CRASH.TR2
        "0cebb72fee215b40b722f0e96106a3a9", // RAPIDS.TR2
        "5c18e057ea40e4c1ceecebc5a9d9cd65", // TRIBOSS.TR2
        "633b0245e14c5b7faab90d1f549ac488", // ROOFS.TR2
        "864a61e73a371dfe0c593f8a233930ec", // SEWER.TR2
        "57a207f21c00362ce60f0ad3f5455a07", // TOWER.TR2
        "69a07074215c08a547a0055b774bacec", // OFFICE.TR2
        "1c4249c0fff242da45728f5177122c10", // STPAUL.TR2
        "62893b73166013b21e2fb3d9c8d9d606", // NEVADA.TR2
        "150b8eae81456cc7b928a40c1c9992ce", // COMPOUND.TR2
        "5e4d62be8333842ef61ac6720b8b77c3", // AREA51.TR2
        "42fd4ca5aa5c02bc4478bbd583689c1a", // ANTARC.TR2
        "d107f57cecfc03b53d00a46a0111dce5", // MINES.TR2
        "436620ba935ffc3cd798cf598ee46ae4", // CITY.TR2
        "ee94ce8722881ab7a1c66cda17bef933", // CHAMBER.TR2
        "1c4249c0fff242da45728f5177122c10", // STPAUL.TR2
        "6efcc71cace4527a0b79128111910eb7", // SCOTLAND.TR2
        "8dc5ff814b23b0654a0b261e501d0320", // WILLSDEN.TR2
        "b99c1af5f462344c267a2141b238fcb4", // CHUNNEL.TR2
        "5d002059f337f165066474a902796d0b", // UNDERSEA.TR2
        "ef8d013cf1fb6495943d9dd74ee09588", // ZOO.TR2
        "ac3b508ae772b3fbd4b16b83b829dfc6", // SLINC.TR2

        // TR3R V1.1+
        "9a1f7162eb7e2b3a57a17e92e0edfbb6", // ANTARC.TR2

        // TR3R V1.4+
        "bd2ded2e22845cd467b78d77aa01bf39", // JUNGLE.TR2
        "93cbd2ad51822c133a92e8ac25e4834f", // TONYBOSS.TR2
        "1a715a8b7c21e1077bdd31882b7bc822", // UNDERSEA.TR2
        "3c68eaa0be796ab6521307f8e19e7503", // ZOO.TR2
    };
}

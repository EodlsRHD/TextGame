# TextGame

Unity Editor version 6000.0.23f1

use Asset
1. TMP
2. NewtonSoft Json | com.unity.nuget.newtonsoft-json
3. DoTween
4. Google Ad Mob 9.2.1 (포함됨)
5. Google Ad Mob Native (포함됨)
6. GPGS 0.11.01(포함됨)

use font
1. NeoDunggeunmoPro-Regular SDF

Preferences

1. android-31
    설치 방법  
    1. 압축 파일을 풀어  아래 경로에 바꿔서 넣는다.
        C:\Program Files\Unity\Hub\Editor\2022.3.50f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\build-tools\~

2. Build-tool 30.0.2
    설치 방법  
    1. 압축 파일을 풀어 아래 경로에 바꿔서 넣는다.
        C:\Program Files\Unity\Hub\Editor\2022.3.50f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platforms\~



* gpgs 설치 후 할일
    1. 파일 열기 : Assets/GooglePlayGames/com.google.play.games/Editor/GooglePlayGamesPluginDependencies.xml
    2. <repository>Packages/com.google.play.games/Editor/m2repository</repository> 이 줄을 3번으로 변경
    3. <repository>Assets/GooglePlayGames/com.google.play.games/Editor/m2repository</repository>
    4. 저장 후 닫기
    5. Unity에서 메뉴 Assets -> External Dependency Manager -> Android Resolver -> Force Resolve 를 한다.
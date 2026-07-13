#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

extern "C" {
    void iOS_Vibrate() {
        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        [generator impactOccurred];
    }
}
